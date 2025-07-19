using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Modding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Facts;
using OperativeRework;
using Owlcat.QA.Validation;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;
using Kingmaker.UnitLogic.Mechanics;
using System.Linq;
using Kingmaker.Utility;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.PubSubSystem;

class ClassesWithGuid
{
	public static List<(Type, string)> Classes = new()
		{
			(typeof(AutoHitComponent), "e79a0bc808afb53b7f9f6f397e4f367c"),
			(typeof(DistributeBuffsAround), "898fb3fa06b9d644d83291ad09a79fdb"),
			(typeof(ApplyBuffStackedByCaster), "7948892518980b855c341fdadac45263"),
			(typeof(RemoveBuffStacksByCaster), "d08c6f8712f9ceb821185aa7f4cbf05c"),
			(typeof(ToggleUiIfCurrentUiOwnerHasFeatureAndNotCaster), "a38f30053d7a26d8cf92f598d2346ebc"),
		};
}

namespace OperativeRework
{
	[AllowedOn(typeof(BlueprintBuff))]
	[TypeId("e79a0bc808afb53b7f9f6f397e4f367c")]
	[Serializable]
	class AutoHitComponent : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateHitChances>
	{
		[SerializeField]
		private RestrictionCalculator Restrictions = new();


		public void OnEventAboutToTrigger(RuleCalculateHitChances evt)
		{
			if (Restrictions.IsPassed(Fact, evt, evt.Ability))
			{
				Owner.GetMechanicFeature(MechanicsFeatureType.AutoHit).Retain(Fact as Buff);
				PFLog.Default.Log("Mawootad - AutoHit - Applying");
			}
			else
			{
				Owner.GetMechanicFeature(MechanicsFeatureType.AutoHit).Release(Fact as Buff);
				PFLog.Default.Log("Mawootad - AutoHit - Removing");
			}
		}

		public void OnEventDidTrigger(RuleCalculateHitChances evt) { }
	}

	[TypeId("898fb3fa06b9d644d83291ad09a79fdb")]
	public class DistributeBuffsAround : ContextAction
	{
		[SerializeField]
		private int Radius;

		[SerializeField]
		private ContextValue StackCount;

		[SerializeField]
		private BlueprintBuffReference Buff;

		[SerializeField]
		private bool Permanent;

		private bool IsCustomDuration { get => !Permanent; }
		[ShowIf("IsCustomDuration")]
		public ContextDurationValue DurationValue;

		[SerializeField]
		private ActionList ActionsOnEmpty;

		public override string GetCaption()
		{
			return "Distribute buffs around";
		}

		protected override void RunAction()
		{
			if (ContextData<UnitHelper.PreviewUnit>.Current) return;
			if (Context.MaybeCaster.IsPreview) return;
			if (Context.MaybeCaster == null) return;
			int stacks = StackCount.Calculate(Context);
			if (stacks <= 0) return;

			List<BaseUnitEntity> list = TempList.Get<BaseUnitEntity>();
			foreach (CustomGridNodeBase customGridNodeBase in GridAreaHelper.GetNodesSpiralAround(Target.NearestNode, Target.SizeRect, Radius, true))
			{
				BaseUnitEntity unit = customGridNodeBase.GetUnit();
				if (unit != null && unit.CombatGroup.IsEnemy(Context.MaybeCaster) && !unit.Features.IsUntargetable && !unit.IsDeadOrUnconscious && !list.HasItem(unit))
				{
					list.Add(unit);
				}
			}

			if (list.Empty())
			{
				ActionsOnEmpty.Run();
				return;
			}

			list.Shuffle(PFStatefulRandom.Mechanics);

			BuffDuration buffDuration = new BuffDuration(Permanent ? null : new Kingmaker.Utility.Rounds?(DurationValue.Calculate(Context)), BuffEndCondition.CombatEnd);

			for (var i = 0; i < list.Count; i++)
			{
				var unit = list[i];
				var unitStacks = stacks / list.Count + (i < stacks % list.Count ? 1 : 0);
				if (unitStacks <= 0) break;

				Buff buff = unit.Buffs.Enumerable.Where(it => it.Blueprint == Buff.Get()).FirstOrDefault(it => it.MaybeContext.MaybeCaster == Context.MaybeCaster);

				if (buff == null)
				{
					buff = unit.Buffs.Add(Buff, Context, buffDuration);
					unitStacks--;
				}


				if (unitStacks >= 1) buff.AddRank(unitStacks);

				if (buff.FirstSource == null)
				{
					buff.AddSource(Context.AssociatedBlueprint);
				}
			}
		}
	}

	[TypeId("7948892518980b855c341fdadac45263")]
	public class ApplyBuffStackedByCaster : ContextActionApplyBuff
	{
		public override string GetCaption() => base.GetCaption().Replace("Apply Buff", "Apply Buff by Owner");

		protected override void RunAction()
		{
			MechanicsContext.Data data = ContextData<MechanicsContext.Data>.Current;

			MechanicsContext mechanicsContext = data?.Context;
			if (mechanicsContext == null)
			{
				LogError(this, "Unable to apply buff: no context found", Array.Empty<object>());
				return;
			}

			MechanicEntity buffTarget = GetBuffTarget(mechanicsContext);
			if (buffTarget == null)
			{
				LogError(this, "Can't apply buff: target is null", Array.Empty<object>());
				return;
			}

			var existingBuff = buffTarget.Buffs.Enumerable.Where(it => it.Blueprint == Buff).FirstOrDefault(it => it.MaybeContext.MaybeCaster == Context.MaybeCaster);

			if (existingBuff == null)
			{
				base.RunAction();
				return;
			}

			int num = this.CalculateRank(mechanicsContext);
			existingBuff.AddRank(num);
		}

		private int CalculateRank(MechanicsContext context)
		{
			return Math.Max(Ranks?.Calculate(context) ?? 1, 1); ;
		}
	}

	[TypeId("d08c6f8712f9ceb821185aa7f4cbf05c")]
	public class RemoveBuffStacksByCaster : ContextAction
	{
		[SerializeField]
		private BlueprintBuffReference Buff;

		[SerializeField]
		private ContextValue StackCount;

		public override string GetCaption() => $"Remove {(StackCount?.IsValueSimple == true && StackCount.Value == 0 ? 1 : StackCount)} stacks of Buff: {(Buff?.Get()?.Name ?? "<not specified>")}";

		protected override void RunAction()
		{
			MechanicEntity mechanicEntity = Context.MaybeCaster;
			if (mechanicEntity == null)
			{
				PFLog.Default.Error("Context.MaybeCaster can't be null!");
				return;
			}

			TargetWrapper target = Target;
			BuffCollection buffCollection = Target?.Entity?.Buffs;
			if (buffCollection == null || buffCollection.Enumerable.Empty())
			{
				return;
			}

			var buff = Buff.Get();
			if (buff == null) return;

			var toRemoveStacks = buffCollection.Enumerable.Where(it => it.Blueprint == buff).FirstOrDefault(it => it.MaybeContext?.MaybeCaster == mechanicEntity);
			if (toRemoveStacks == null) return;

			toRemoveStacks.RemoveRank(Math.Max(StackCount.Calculate(Context), 1));
		}
	}

	[TypeId("a38f30053d7a26d8cf92f598d2346ebc")]
	public class ToggleUiIfCurrentUiOwnerHasFeatureAndNotCaster : UnitFactComponentDelegate, IContinueTurnHandler, ITurnStartHandler, IInterruptTurnStartHandler, IUnitBuffHandler
	{
		[SerializeField]
		private BlueprintBuffReference UiOnBuff;

		[SerializeField]
		private BlueprintBuffReference UiOffBuff;

		[SerializeField]
		private BlueprintFeatureReference FeatureTrigger;

		private Buff GetBuffFromCaster(BlueprintBuff buff) => Context?.MainTarget?.Entity?.Buffs?.Enumerable?.Where(it => it.Blueprint == buff)?.FirstOrDefault(it => it.MaybeContext?.MaybeCaster == Context?.MaybeCaster);

		private void RemoveBuffs()
		{
			GetBuffFromCaster(UiOnBuff.Get())?.Remove();
			GetBuffFromCaster(UiOffBuff.Get())?.Remove();
		}

		private void UpdateBuffs()
		{
			if (Owner.IsPreviewUnit) return;
			var entity = EventInvokerExtensions.MechanicEntity;
			if (entity == null) return;
			bool uiOn = !entity.Facts.Contains(FeatureTrigger.Get()) || Fact.MaybeContext?.MaybeCaster == entity;
			RemoveBuffs();

			var data = ContextData<Buff.Data>.Current.Buff;
			var target = Context?.MainTarget?.Entity;
			if (target == null) return;
			Rounds? duration = data.IsPermanent ? null : new Rounds(data.DurationInRounds);
			var buff = target.Buffs.Add(uiOn ? UiOnBuff.Get() : UiOffBuff.Get(), Context, new(duration));
			buff.AddRank(buff.Rank - 1);
		}

		public void HandleUnitContinueTurn(bool isTurnBased) => UpdateBuffs();

		public void HandleUnitStartInterruptTurn(InterruptionData interruptionData) => UpdateBuffs();

		public void HandleUnitStartTurn(bool isTurnBased) => UpdateBuffs();

		protected override void OnFactAttached() => UpdateBuffs();

		protected override void OnFactDetached() => RemoveBuffs();

		public void HandleBuffDidAdded(Buff buff)
		{
			if (buff == ContextData<Buff.Data>.Current.Buff) UpdateBuffs();
		}

		public void HandleBuffDidRemoved(Buff buff)
		{
			if (buff == ContextData<Buff.Data>.Current.Buff) RemoveBuffs();
		}

		public void HandleBuffRankIncreased(Buff buff)
		{
			if (buff == ContextData<Buff.Data>.Current.Buff) UpdateBuffs();
		}

		public void HandleBuffRankDecreased(Buff buff)
		{
			if (buff == ContextData<Buff.Data>.Current.Buff) UpdateBuffs();
		}
	}
}
