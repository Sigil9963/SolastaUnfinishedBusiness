﻿using System.Collections;
using System.Collections.Generic;
using SolastaUnfinishedBusiness.Api.GameExtensions;
using SolastaUnfinishedBusiness.Api.Helpers;
using SolastaUnfinishedBusiness.Builders;
using SolastaUnfinishedBusiness.Builders.Features;
using SolastaUnfinishedBusiness.CustomBehaviors;
using SolastaUnfinishedBusiness.CustomInterfaces;
using SolastaUnfinishedBusiness.CustomUI;
using SolastaUnfinishedBusiness.CustomValidators;
using SolastaUnfinishedBusiness.Models;
using SolastaUnfinishedBusiness.Properties;
using static FeatureDefinitionAttributeModifier;
using static RuleDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.ConditionDefinitions;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.FeatureDefinitionDamageAffinitys;
using static SolastaUnfinishedBusiness.Api.DatabaseHelper.SpellDefinitions;

namespace SolastaUnfinishedBusiness.Spells;

internal static partial class SpellBuilders
{
    #region LEVEL 01

    internal static SpellDefinition BuildCausticZap()
    {
        const string NAME = "CausticZap";

        var effectDescription = EffectDescriptionBuilder
            .Create()
            .SetTargetingData(Side.Enemy, RangeType.RangeHit, 18, TargetType.IndividualsUnique)
            .SetDurationData(DurationType.Round, 1, TurnOccurenceType.EndOfSourceTurn)
            .SetEffectAdvancement(EffectIncrementMethod.PerAdditionalSlotLevel, 1, 1)
            .SetParticleEffectParameters(ShockingGrasp)
            .SetEffectForms(
                EffectFormBuilder
                    .Create()
                    .SetDamageForm(DamageTypeAcid, 1, DieType.D4)
                    .Build(),
                EffectFormBuilder
                    .Create()
                    .SetDamageForm(DamageTypeLightning, 1, DieType.D6)
                    .Build(),
                EffectFormBuilder
                    .Create()
                    .SetConditionForm(ConditionDazzled, ConditionForm.ConditionOperation.Add)
                    .Build())
            .Build();

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, Sprites.GetSprite(NAME, Resources.CausticZap, 128))
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolEvocation)
            .SetSpellLevel(1)
            .SetSomaticComponent(true)
            .SetVerboseComponent(true)
            .SetCastingTime(ActivationTime.Action)
            .SetEffectDescription(effectDescription)
            .AddToDB();

        return spell;
    }

    internal static SpellDefinition BuildChromaticOrb()
    {
        const string NAME = "ChromaticOrb";

        var sprite = Sprites.GetSprite(NAME, Resources.ChromaticOrb, 128);
        var subSpells = new SpellDefinition[6];
        var particleTypes = new[] { AcidSplash, ConeOfCold, FireBolt, LightningBolt, PoisonSpray, Thunderwave };
        var damageTypes = new[]
        {
            DamageTypeAcid, DamageTypeCold, DamageTypeFire, DamageTypeLightning, DamageTypePoison, DamageTypeThunder
        };

        for (var i = 0; i < subSpells.Length; i++)
        {
            var damageType = damageTypes[i];
            var particleType = particleTypes[i];
            var title = Gui.Localize($"Tooltip/&Tag{damageType}Title");
            var spell = SpellDefinitionBuilder
                .Create(NAME + damageType)
                .SetGuiPresentation(
                    title,
                    Gui.Format("Spell/&SubSpellChromaticOrbDescription", title),
                    sprite)
                .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolEvocation)
                .SetSpellLevel(1)
                .SetMaterialComponent(MaterialComponentType.Specific)
                .SetSpecificMaterialComponent(TagsDefinitions.ItemTagDiamond, 50, false)
                .SetVocalSpellSameType(VocalSpellSemeType.Attack)
                .SetCastingTime(ActivationTime.Action)
                .SetEffectDescription(EffectDescriptionBuilder
                    .Create()
                    .SetTargetFiltering(TargetFilteringMethod.CharacterOnly)
                    .SetTargetingData(Side.Enemy, RangeType.RangeHit, 12, TargetType.IndividualsUnique)
                    .SetDurationData(DurationType.Instantaneous)
                    .SetEffectForms(EffectFormBuilder.Create()
                        .SetDamageForm(damageType, 3, DieType.D8)
                        .Build())
                    .SetEffectAdvancement(EffectIncrementMethod.PerAdditionalSlotLevel, additionalDicePerIncrement: 1)
                    .SetParticleEffectParameters(particleType)
                    .SetSpeed(SpeedType.CellsPerSeconds, 8.5f)
                    .SetupImpactOffsets(offsetImpactTimePerTarget: 0.1f)
                    .Build())
                .AddToDB();

            subSpells[i] = spell;
        }

        return SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, sprite)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolEvocation)
            .SetSpellLevel(1)
            .SetMaterialComponent(MaterialComponentType.Specific)
            .SetSpecificMaterialComponent(TagsDefinitions.ItemTagDiamond, 50, false)
            .SetVocalSpellSameType(VocalSpellSemeType.Attack)
            .SetCastingTime(ActivationTime.Action)
            .SetSubSpells(subSpells)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetFiltering(TargetFilteringMethod.CharacterOnly)
                .SetTargetingData(Side.Enemy, RangeType.RangeHit, 12, TargetType.IndividualsUnique)
                .SetDurationData(DurationType.Instantaneous)
                .SetEffectAdvancement(EffectIncrementMethod.PerAdditionalSlotLevel,
                    additionalDicePerIncrement: 1)
                .SetSpeed(SpeedType.CellsPerSeconds, 8.5f)
                .SetupImpactOffsets(offsetImpactTimePerTarget: 0.1f)
                .Build())
            .AddToDB();
    }


    internal static SpellDefinition BuildEarthTremor()
    {
        const string NAME = "EarthTremor";

        var spriteReference = Sprites.GetSprite(NAME, Resources.EarthTremor, 128, 128);

        var rubbleProxy = EffectProxyDefinitionBuilder
            .Create(EffectProxyDefinitions.ProxyGrease, "EarthTremorRubbleProxy")
            .AddToDB();

        var effectDescription = EffectDescriptionBuilder
            .Create()
            .SetEffectAdvancement(EffectIncrementMethod.PerAdditionalSlotLevel, 1, 0, 1)
            .SetSavingThrowData(
                false,
                AttributeDefinitions.Dexterity,
                false,
                EffectDifficultyClassComputation.SpellCastingFeature)
            .SetDurationData(DurationType.Minute, 1)
            .SetParticleEffectParameters(Grease)
            .SetTargetingData(Side.All, RangeType.Distance, 24, TargetType.Cylinder, 2, 1)
            .SetEffectAdvancement(EffectIncrementMethod.PerAdditionalSlotLevel, additionalDicePerIncrement: 1)
            .SetEffectForms(
                EffectFormBuilder
                    .Create()
                    .SetSummonEffectProxyForm(rubbleProxy)
                    .Build(),
                EffectFormBuilder
                    .Create()
                    .SetMotionForm(MotionForm.MotionType.FallProne, 1)
                    .HasSavingThrow(EffectSavingThrowType.Negates)
                    .Build(),
                EffectFormBuilder
                    .Create()
                    .SetDamageForm(DamageTypeBludgeoning, 1, DieType.D6)
                    .HasSavingThrow(EffectSavingThrowType.HalfDamage)
                    .Build(),
                Grease.EffectDescription.EffectForms.Find(e => e.formType == EffectForm.EffectFormType.Topology))
            .Build();

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, spriteReference)
            .SetEffectDescription(effectDescription)
            .SetCastingTime(ActivationTime.Action)
            .SetSpellLevel(1)
            .SetVocalSpellSameType(VocalSpellSemeType.Attack)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolEvocation)
            .AddToDB();

        return spell;
    }

    internal static SpellDefinition BuildEnsnaringStrike()
    {
        const string NAME = "EnsnaringStrike";

        var ensnared = ConditionDefinitionBuilder
            .Create(ConditionRestrainedByEntangle, $"Condition{NAME}Enemy")
            .SetSpecialDuration(DurationType.Minute, 1, TurnOccurenceType.StartOfTurn)
            .SetRecurrentEffectForms(EffectFormBuilder.Create()
                .SetDamageForm(DamageTypePiercing, 1, DieType.D6)
                .Build())
            .AddToDB();

        var additionalDamageEnsnaringStrike = FeatureDefinitionAdditionalDamageBuilder
            .Create($"AdditionalDamage{NAME}")
            .SetGuiPresentation(Category.Feature)
            .SetNotificationTag(NAME)
            .SetDamageDice(DieType.D6, 1)
            .SetSpecificDamageType(DamageTypePiercing)
            .SetAdvancement(AdditionalDamageAdvancement.SlotLevel)
            .SetSavingThrowData(
                EffectDifficultyClassComputation.SpellCastingFeature,
                EffectSavingThrowType.None,
                AttributeDefinitions.Strength)
            .SetCustomSubFeatures(new AdditionalEffectFormOnDamageHandler((attacker, _, provider) =>
                    new List<EffectForm>
                    {
                        EffectFormBuilder.Create()
                            .SetConditionForm(ensnared, ConditionForm.ConditionOperation.Add)
                            .HasSavingThrow(EffectSavingThrowType.Negates)
                            .OverrideSavingThrowInfo(AttributeDefinitions.Strength,
                                GameLocationBattleManagerTweaks.ComputeSavingThrowDC(attacker.RulesetCharacter,
                                    provider))
                            .Build()
                    }),
                ValidatorsRestrictedContext.WeaponAttack)
            .AddToDB();

        var conditionEnsnaringStrike = ConditionDefinitionBuilder
            .Create($"Condition{NAME}")
            .SetGuiPresentation(NAME, Category.Spell, ConditionBrandingSmite)
            .AddFeatures(additionalDamageEnsnaringStrike)
            .SetSpecialInterruptions(ConditionInterruption.AttacksAndDamages)
            .SetPossessive()
            .AddToDB();

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, Sprites.GetSprite("EnsnaringStrike", Resources.EnsnaringStrike, 128))
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolConjuration)
            .SetSpellLevel(1)
            .SetMaterialComponent(MaterialComponentType.None)
            .SetVocalSpellSameType(VocalSpellSemeType.Buff)
            .SetCastingTime(ActivationTime.BonusAction)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetEffectForms(EffectFormBuilder.Create()
                    .SetConditionForm(conditionEnsnaringStrike, ConditionForm.ConditionOperation.Add)
                    .Build())
                .SetDurationData(DurationType.Minute, 1)
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetEffectAdvancement(EffectIncrementMethod.PerAdditionalSlotLevel, additionalDicePerIncrement: 1)
                .Build())
            .SetRequiresConcentration(true)
            .AddToDB();

        return spell;
    }

    internal const string OwlFamiliar = "OwlFamiliar";

    internal static SpellDefinition BuildFindFamiliar()
    {
        var familiarMonster = MonsterDefinitionBuilder
            .Create(MonsterDefinitions.Eagle_Matriarch, OwlFamiliar)
            .SetOrUpdateGuiPresentation(Category.Monster)
            .SetFeatures(
                FeatureDefinitionSenses.SenseNormalVision,
                FeatureDefinitionSenses.SenseDarkvision24,
                FeatureDefinitionMoveModes.MoveModeMove2,
                FeatureDefinitionMoveModes.MoveModeFly12,
                FeatureDefinitionAbilityCheckAffinitys.AbilityCheckAffinityKeenSight,
                FeatureDefinitionAbilityCheckAffinitys.AbilityCheckAffinityKeenHearing,
                FeatureDefinitionCombatAffinitys.CombatAffinityFlyby,
                FeatureDefinitionMovementAffinitys.MovementAffinityNoClimb,
                FeatureDefinitionMovementAffinitys.MovementAffinityNoSpecialMoves,
                FeatureDefinitionConditionAffinitys.ConditionAffinityProneImmunity)
            .SetMonsterPresentation(
                MonsterPresentationBuilder.Create()
                    .SetAllPrefab(MonsterDefinitions.Eagle_Matriarch.MonsterPresentation)
                    .SetPhantom()
                    .SetModelScale(0.5f)
                    .SetHasMonsterPortraitBackground(true)
                    .SetCanGeneratePortrait(true)
                    .Build())
            .ClearAttackIterations()
            .SetSkillScores((SkillDefinitions.Perception, 3), (SkillDefinitions.Stealth, 3))
            .SetArmorClass(11)
            .SetAbilityScores(3, 13, 8, 2, 12, 7)
            .SetHitDice(DieType.D4, 1)
            .SetStandardHitPoints(5)
            .SetSizeDefinition(CharacterSizeDefinitions.Tiny)
            .SetAlignment("Neutral")
            .SetCharacterFamily(CharacterFamilyDefinitions.Fey.name)
            .SetChallengeRating(0)
            .SetDroppedLootDefinition(null)
            .SetDefaultBattleDecisionPackage(DecisionPackageDefinitions.DefaultSupportCasterWithBackupAttacksDecisions)
            .SetFullyControlledWhenAllied(true)
            .SetDefaultFaction(FactionDefinitions.Party)
            .SetBestiaryEntry(BestiaryDefinitions.BestiaryEntry.None)
            .AddFeatures(CharacterContext.FeatureDefinitionPowerHelpAction)
            .AddToDB();

        var spell = SpellDefinitionBuilder.Create(Fireball, "FindFamiliar")
            .SetGuiPresentation(Category.Spell, AnimalFriendship)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolConjuration)
            .SetMaterialComponent(MaterialComponentType.Specific)
            .SetSomaticComponent(true)
            .SetVerboseComponent(true)
            .SetSpellLevel(1)
            .SetUniqueInstance()
            .SetCastingTime(ActivationTime.Hours1)
            .SetRitualCasting(ActivationTime.Hours1)
            .SetEffectDescription(
                EffectDescriptionBuilder
                    .Create()
                    .SetDurationData(DurationType.Permanent)
                    .SetTargetingData(Side.Ally, RangeType.Distance, 2, TargetType.Position)
                    .SetParticleEffectParameters(ConjureAnimalsOneBeast)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetSummonCreatureForm(1, familiarMonster.Name)
                            .Build())
                    .Build())
            .SetCustomSubFeatures(SkipEffectRemovalOnLocationChange.Always)
            .AddToDB();

        GlobalUniqueEffects.AddToGroup(GlobalUniqueEffects.Group.Familiar, spell);

        return spell;
    }
    internal static SpellDefinition BuildIceKnife()
    {
        const string NAME = "IceKnife";

        var additionalDamageKnifed = FeatureDefinitionAdditionalDamageBuilder
            .Create($"AdditionDamage{NAME}")
            .SetSavingThrowData(
                EffectDifficultyClassComputation.SpellCastingFeature,
                EffectSavingThrowType.HalfDamage,
                savingThrowAbility: AttributeDefinitions.Dexterity)
            .SetDamageDice(DieType.D6, 2)
            .SetSpecificDamageType(DamageTypeCold)
            .AddToDB();

        var conditionKnifed = ConditionDefinitionBuilder
            .Create(ConditionCursed, "ConditionKnifed")
            .SetOrUpdateGuiPresentation(Category.Condition)
            .SetSpecialDuration(DurationType.Instantaneous)
            .SetFeatures(additionalDamageKnifed)
            //.radius
            .AddToDB();

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, Sprites.GetSprite(NAME, Resources.AcidBolt, 128))
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.All, RangeType.RangeHit, 12, TargetType.Individuals, 2)
                .SetEffectAdvancement(EffectIncrementMethod.PerAdditionalSlotLevel, additionalDicePerIncrement: 1)
                .SetParticleEffectParameters(RayOfFrost.EffectDescription.EffectParticleParameters)
                .AddEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetDamageForm(DamageTypePiercing, 1, DieType.D10)
                        .Build(),
                   EffectFormBuilder
                        .Create()
                        .SetConditionForm(conditionKnifed, ConditionForm.ConditionOperation.Add)
                        .Build())
                .Build())
            .SetCastingTime(ActivationTime.Action)
            .SetSpellLevel(1)
            .SetVerboseComponent(false)
            .SetSomaticComponent(true)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolConjuration)
            .AddToDB();

        return spell;
    }
    internal static SpellDefinition BuildMule()
    {
        const string NAME = "Mule";

        var effectDescription = EffectDescriptionBuilder
            .Create()
            .SetTargetingData(Side.Ally, RangeType.Touch, 1, TargetType.IndividualsUnique)
            .SetDurationData(DurationType.Hour, 8)
            .SetParticleEffectParameters(ExpeditiousRetreat)
            .SetEffectForms(
                EffectFormBuilder
                    .Create()
                    .SetConditionForm(
                        ConditionDefinitionBuilder
                            .Create($"Condition{NAME}")
                            .SetGuiPresentation(Category.Condition, Longstrider)
                            .SetFeatures(
                                FeatureDefinitionMovementAffinityBuilder
                                    .Create($"MovementAffinity{NAME}")
                                    .SetGuiPresentationNoContent(true)
                                    .SetImmunities(true, true)
                                    .AddToDB(),
                                FeatureDefinitionEquipmentAffinityBuilder
                                    .Create($"EquipmentAffinity{NAME}")
                                    .SetGuiPresentationNoContent(true)
                                    .SetAdditionalCarryingCapacity(20)
                                    .AddToDB())
                            .AddToDB(),
                        ConditionForm.ConditionOperation.Add,
                        false,
                        false,
                        ConditionJump.AdditionalCondition)
                    .Build())
            .Build();

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell,
                Sprites.GetSprite("Mule", Resources.Mule, 128))
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolTransmutation)
            .SetSpellLevel(1)
            .SetCastingTime(ActivationTime.Action)
            .SetConcentrationAction(ActionDefinitions.ActionParameter.None)
            .SetEffectDescription(effectDescription)
            .AddToDB();

        return spell;
    }

    internal static SpellDefinition BuildRadiantMotes()
    {
        const string NAME = "RadiantMotes";

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, Sprites.GetSprite(NAME, Resources.RadiantMotes, 128))
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolEvocation)
            .SetSpellLevel(1)
            .SetMaterialComponent(MaterialComponentType.None)
            .SetVocalSpellSameType(VocalSpellSemeType.Attack)
            .SetCastingTime(ActivationTime.Action)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetFiltering(TargetFilteringMethod.AllCharacterAndGadgets)
                .SetTargetingData(Side.Enemy, RangeType.RangeHit, 12, TargetType.Individuals, 4)
                .SetDurationData(DurationType.Minute, 1)
                .SetEffectForms(EffectFormBuilder.Create()
                    .SetDamageForm(DamageTypeRadiant, 1, DieType.D4)
                    .Build())
                .SetEffectAdvancement(EffectIncrementMethod.PerAdditionalSlotLevel, 1, 1)
                .SetParticleEffectParameters(Sparkle)
                .SetSpeed(SpeedType.CellsPerSeconds, 20)
                .SetupImpactOffsets(offsetImpactTimePerTarget: 0.1f)
                .Build())
            .AddToDB();

        return spell;
    }

    internal static SpellDefinition BuildSearingSmite()
    {
        const string NAME = "SearingSmite";

        var additionalDamageSearingSmite = FeatureDefinitionAdditionalDamageBuilder
            .Create($"AdditionalDamage{NAME}")
            .SetGuiPresentation(Category.Feature)
            .SetNotificationTag(NAME)
            .SetCustomSubFeatures(ValidatorsRestrictedContext.WeaponAttack)
            .SetDamageDice(DieType.D6, 1)
            .SetSpecificDamageType(DamageTypeFire)
            .SetAdvancement(AdditionalDamageAdvancement.SlotLevel, 1)
            .SetSavingThrowData(
                EffectDifficultyClassComputation.SpellCastingFeature,
                EffectSavingThrowType.None)
            .SetConditionOperations(
                new ConditionOperationDescription
                {
                    hasSavingThrow = true,
                    canSaveToCancel = true,
                    saveAffinity = EffectSavingThrowType.Negates,
                    saveOccurence = TurnOccurenceType.StartOfTurn,
                    conditionDefinition = ConditionDefinitionBuilder
                        .Create(ConditionOnFire, $"Condition{NAME}Enemy")
                        .AddToDB(),
                    operation = ConditionOperationDescription.ConditionOperation.Add
                })
            .AddToDB();

        var conditionSearingSmite = ConditionDefinitionBuilder
            .Create($"Condition{NAME}")
            .SetGuiPresentation(NAME, Category.Spell, ConditionBrandingSmite)
            .SetPossessive()
            .SetFeatures(additionalDamageSearingSmite)
            .SetSpecialInterruptions(ConditionInterruption.AttacksAndDamages)
            .AddToDB();

        var spell = SpellDefinitionBuilder
            .Create(BrandingSmite, NAME)
            .SetGuiPresentation(Category.Spell, Sprites.GetSprite(NAME, Resources.SearingSmite, 128))
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolEvocation)
            .SetSpellLevel(1)
            .SetCastingTime(ActivationTime.BonusAction)
            .SetVerboseComponent(true)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetDurationData(DurationType.Minute, 1)
                .SetEffectForms(EffectFormBuilder.Create()
                    .SetConditionForm(conditionSearingSmite, ConditionForm.ConditionOperation.Add)
                    .Build())
                .SetEffectAdvancement(EffectIncrementMethod.PerAdditionalSlotLevel, additionalDicePerIncrement: 1)
                .Build())
            .AddToDB();

        return spell;
    }

    internal static SpellDefinition BuildSkinOfRetribution()
    {
        const string NAME = "SkinOfRetribution";
        const int TEMP_HP_PER_LEVEL = 5;

        var spriteReferenceCondition = Sprites.GetSprite("ConditionMirrorImage", Resources.ConditionMirrorImage, 32);

        var subSpells = new List<SpellDefinition>();
        var damageTypes = new[]
        {
            DamageTypeAcid, DamageTypeCold, DamageTypeFire, DamageTypeLightning, DamageTypePoison, DamageTypeThunder
        };

        const string SUB_SPELL_DESCRIPTION = $"Spell/&SubSpell{NAME}Description";
        const string SUB_SPELL_CONDITION_DESCRIPTION = $"Condition/&Condition{NAME}Description";
        const string SUB_SPELL_CONDITION_TITLE = $"Condition/&Condition{NAME}Title";

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var damageType in damageTypes)
        {
            var title = Gui.Localize($"Tooltip/&Tag{damageType}Title");

            var powerSkinOfRetribution = FeatureDefinitionPowerBuilder
                .Create($"Power{NAME}{damageType}")
                .SetGuiPresentationNoContent(true)
                .SetUsesFixed(ActivationTime.NoCost)
                .SetEffectDescription(
                    EffectDescriptionBuilder
                        .Create()
                        .SetEffectForms(
                            EffectFormBuilder
                                .Create()
                                .SetDamageForm(damageType, bonusDamage: TEMP_HP_PER_LEVEL)
                                .Build())
                        .Build())
                .SetCustomSubFeatures(new ModifyMagicEffectSkinOfRetribution())
                .AddToDB();

            var damageSkinOfRetribution = FeatureDefinitionDamageAffinityBuilder
                .Create($"DamageAffinity{NAME}{damageType}")
                .SetGuiPresentationNoContent(true)
                .SetDamageAffinityType(DamageAffinityType.None)
                .SetRetaliate(powerSkinOfRetribution, 1, true)
                .AddToDB();

            var conditionSkinOfRetribution = ConditionDefinitionBuilder
                .Create($"Condition{NAME}{damageType}")
                .SetGuiPresentation(SUB_SPELL_CONDITION_TITLE,
                    Gui.Format(SUB_SPELL_CONDITION_DESCRIPTION, title), spriteReferenceCondition
                )
                .SetSilent(Silent.WhenAdded)
                .SetPossessive()
                .SetFeatures(damageSkinOfRetribution)
                .AddToDB();

            var spell = SpellDefinitionBuilder
                .Create(NAME + damageType)
                .SetGuiPresentation(title, Gui.Format(SUB_SPELL_DESCRIPTION, title),
                    Sprites.GetSprite(NAME, Resources.SkinOfRetribution, 128))
                .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolAbjuration)
                .SetVerboseComponent(false)
                .SetVocalSpellSameType(VocalSpellSemeType.Defense)
                .SetSpellLevel(1)
                .SetEffectDescription(EffectDescriptionBuilder
                    .Create()
                    .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                    .SetDurationData(DurationType.Hour, 1)
                    .SetEffectForms(
                        EffectFormBuilder
                            .Create()
                            .SetTempHpForm(TEMP_HP_PER_LEVEL)
                            .Build(),
                        EffectFormBuilder
                            .Create()
                            .SetConditionForm(conditionSkinOfRetribution, ConditionForm.ConditionOperation.Add)
                            .Build())
                    .SetEffectAdvancement(EffectIncrementMethod.PerAdditionalSlotLevel,
                        additionalTempHpPerIncrement: TEMP_HP_PER_LEVEL)
                    .SetParticleEffectParameters(Blur)
                    .Build())
                .AddToDB();

            subSpells.Add(spell);
        }

        return SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, Sprites.GetSprite(NAME, Resources.SkinOfRetribution, 128))
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolAbjuration)
            .SetVerboseComponent(false)
            .SetVocalSpellSameType(VocalSpellSemeType.Defense)
            .SetSpellLevel(1)
            .SetSubSpells(subSpells.ToArray())
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetDurationData(DurationType.Hour, 1)
                .SetEffectAdvancement(EffectIncrementMethod.PerAdditionalSlotLevel,
                    additionalTempHpPerIncrement: TEMP_HP_PER_LEVEL)
                .SetParticleEffectParameters(Blur)
                .Build())
            .AddToDB();
    }

    internal static SpellDefinition BuildThunderousSmite()
    {
        const string NAME = "ThunderousSmite";

        var power = FeatureDefinitionPowerBuilder
            .Create($"Power{NAME}Push")
            .SetGuiPresentationNoContent(true)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetTargetingData(Side.Enemy, RangeType.Touch, 1, TargetType.IndividualsUnique)
                .SetEffectForms(
                    EffectFormBuilder.Create()
                        .SetMotionForm(MotionForm.MotionType.PushFromOrigin, 2)
                        .Build(),
                    EffectFormBuilder.Create()
                        .SetMotionForm(MotionForm.MotionType.FallProne)
                        .Build()
                )
                .Build())
            .AddToDB();

        var additionalDamageThunderousSmite = FeatureDefinitionAdditionalDamageBuilder
            .Create($"AdditionalDamage{NAME}")
            .SetGuiPresentation(Category.Feature)
            .SetNotificationTag(NAME)
            .SetCustomSubFeatures(ValidatorsRestrictedContext.WeaponAttack)
            .SetDamageDice(DieType.D6, 2)
            .SetSpecificDamageType(DamageTypeThunder)
            .SetSavingThrowData(
                EffectDifficultyClassComputation.SpellCastingFeature,
                EffectSavingThrowType.None,
                AttributeDefinitions.Strength)
            .SetConditionOperations(
                new ConditionOperationDescription
                {
                    hasSavingThrow = true,
                    canSaveToCancel = true,
                    saveAffinity = EffectSavingThrowType.Negates,
                    saveOccurence = TurnOccurenceType.StartOfTurn,
                    conditionDefinition = ConditionDefinitionBuilder
                        .Create($"Condition{NAME}Enemy")
                        .SetGuiPresentationNoContent(true)
                        .SetSilent(Silent.WhenAddedOrRemoved)
                        .SetCustomSubFeatures(new ConditionUsesPowerOnTarget(power))
                        .AddToDB(),
                    operation = ConditionOperationDescription.ConditionOperation.Add
                })
            .AddToDB();

        var conditionThunderousSmite = ConditionDefinitionBuilder
            .Create($"Condition{NAME}")
            .SetGuiPresentation(NAME, Category.Spell, ConditionBrandingSmite)
            .SetPossessive()
            .SetFeatures(additionalDamageThunderousSmite)
            .SetSpecialInterruptions(ConditionInterruption.AttacksAndDamages)
            .AddToDB();

        var spell = SpellDefinitionBuilder
            .Create(BrandingSmite, NAME)
            .SetGuiPresentation(Category.Spell, Sprites.GetSprite(NAME, Resources.ThunderousSmite, 128))
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolEvocation)
            .SetSpellLevel(1)
            .SetCastingTime(ActivationTime.BonusAction)
            .SetVerboseComponent(true)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetDurationData(DurationType.Minute, 1)
                .SetEffectForms(EffectFormBuilder.Create()
                    .SetConditionForm(conditionThunderousSmite, ConditionForm.ConditionOperation.Add)
                    .Build())
                .Build())
            .AddToDB();

        return spell;
    }

    internal static SpellDefinition BuildWrathfulSmite()
    {
        const string NAME = "WrathfulSmite";

        var additionalDamageWrathfulSmite = FeatureDefinitionAdditionalDamageBuilder
            .Create($"AdditionalDamage{NAME}")
            .SetGuiPresentation(Category.Feature)
            .SetNotificationTag(NAME)
            .SetCustomSubFeatures(ValidatorsRestrictedContext.WeaponAttack)
            .SetDamageDice(DieType.D6, 1)
            .SetSpecificDamageType(DamageTypePsychic)
            .SetAdvancement(AdditionalDamageAdvancement.SlotLevel, 1)
            .SetSavingThrowData(
                EffectDifficultyClassComputation.SpellCastingFeature,
                EffectSavingThrowType.None,
                AttributeDefinitions.Wisdom)
            .SetConditionOperations(
                new ConditionOperationDescription
                {
                    hasSavingThrow = true,
                    canSaveToCancel = true,
                    saveAffinity = EffectSavingThrowType.Negates,
                    saveOccurence = TurnOccurenceType.StartOfTurn,
                    conditionDefinition = ConditionDefinitionBuilder
                        .Create(ConditionDefinitions.ConditionFrightened, $"Condition{NAME}Enemy")
                        .SetSpecialDuration(DurationType.Minute, 1, TurnOccurenceType.StartOfTurn)
                        .SetParentCondition(ConditionDefinitions.ConditionFrightened)
                        .AddToDB(),
                    operation = ConditionOperationDescription.ConditionOperation.Add
                })
            .AddToDB();

        var conditionWrathfulSmite = ConditionDefinitionBuilder
            .Create($"Condition{NAME}")
            .SetGuiPresentation(NAME, Category.Spell, ConditionBrandingSmite)
            .SetPossessive()
            .SetFeatures(additionalDamageWrathfulSmite)
            .SetSpecialInterruptions(ConditionInterruption.AttacksAndDamages)
            .AddToDB();

        var spell = SpellDefinitionBuilder
            .Create(BrandingSmite, NAME)
            .SetGuiPresentation(Category.Spell, Sprites.GetSprite(NAME, Resources.WrathfulSmite, 128))
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolEvocation)
            .SetSpellLevel(1)
            .SetCastingTime(ActivationTime.BonusAction)
            .SetVerboseComponent(true)
            .SetEffectDescription(EffectDescriptionBuilder.Create()
                .SetTargetingData(Side.Ally, RangeType.Self, 0, TargetType.Self)
                .SetDurationData(DurationType.Minute, 1)
                .SetEffectForms(EffectFormBuilder.Create()
                    .SetConditionForm(conditionWrathfulSmite, ConditionForm.ConditionOperation.Add)
                    .Build())
                .Build())
            .AddToDB();

        return spell;
    }

    internal static SpellDefinition BuildSanctuary()
    {
        const string NAME = "Sanctuary";

        var conditionSanctuaryArmorClass = ConditionDefinitionBuilder
            .Create($"Condition{NAME}ArmorClass")
            .SetGuiPresentationNoContent(true)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .AddFeatures(FeatureDefinitionAttributeModifierBuilder
                .Create($"AttributeModifier{NAME}ArmorClass")
                .SetGuiPresentationNoContent(true)
                .SetModifier(AttributeModifierOperation.Additive, AttributeDefinitions.ArmorClass, 30)
                .AddToDB())
            .AddSpecialInterruptions(ConditionInterruption.Attacked)
            .AddToDB();

        var conditionSanctuaryDamageResistance = ConditionDefinitionBuilder
            .Create($"Condition{NAME}DamageResistance")
            .SetGuiPresentationNoContent(true)
            .SetSilent(Silent.WhenAddedOrRemoved)
            .AddFeatures(
                DamageAffinityAcidResistance,
                DamageAffinityBludgeoningResistance,
                DamageAffinityColdResistance,
                DamageAffinityFireResistance,
                DamageAffinityForceDamageResistance,
                DamageAffinityLightningResistance,
                DamageAffinityNecroticResistance,
                DamageAffinityPiercingResistance,
                DamageAffinityPoisonResistance,
                DamageAffinityPsychicResistance,
                DamageAffinityRadiantResistance,
                DamageAffinitySlashingResistance,
                DamageAffinityThunderResistance)
            .AddSpecialInterruptions(ConditionInterruption.Attacked)
            .AddToDB();

        //Attack possible is skipped when crit, so I am just going to halve the damage on critical
        var featureSanctuary = FeatureDefinitionBuilder
            .Create($"Feature{NAME}")
            .SetGuiPresentationNoContent(true)
            .SetCustomSubFeatures(
                new SanctuaryBeforeAttackHitPossible(conditionSanctuaryArmorClass),
                new PhysicalAttackBeforeHitConfirmedOnMeSanctuary(conditionSanctuaryDamageResistance))
            .AddToDB();

        var conditionSanctuary = ConditionDefinitionBuilder
            .Create($"Condition{NAME}")
            .SetGuiPresentation(Category.Condition, ConditionDivineFavor)
            .AddSpecialInterruptions(ConditionInterruption.Attacks)
            .SetFeatures(featureSanctuary)
            .AddToDB();

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell,
                Sprites.GetSprite("Sanctuary", Resources.Sanctuary, 128))
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolAbjuration)
            .SetSpellLevel(1)
            .SetCastingTime(ActivationTime.BonusAction)
            .SetVerboseComponent(true)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.Ally, RangeType.Distance, 6, TargetType.IndividualsUnique)
                .SetDurationData(DurationType.Minute, 1)
                .SetEffectForms(EffectFormBuilder
                    .Create()
                    .SetConditionForm(conditionSanctuary, ConditionForm.ConditionOperation.Add)
                    .Build())
                .Build())
            .AddToDB();

        return spell;
    }

    internal static SpellDefinition BuildGiftOfAlacrity()
    {
        const string NAME = "GiftOfAlacrity";

        var featureGiftOfAlacrity = FeatureDefinitionBuilder
            .Create("FeatureGiftOfAlacrity")
            .SetGuiPresentation("ConditionGiftOfAlacrity", Category.Condition)
            .AddToDB();

        featureGiftOfAlacrity.SetCustomSubFeatures(new InitiativeEndListenerGiftOfAlacrity(featureGiftOfAlacrity));

        var conditionAlacrity = ConditionDefinitionBuilder
            .Create(ConditionBlessed, "ConditionGiftOfAlacrity")
            .SetOrUpdateGuiPresentation(Category.Condition)
            .SetPossessive()
            .SetFeatures(featureGiftOfAlacrity)
            .AddToDB();

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, CalmEmotions.GuiPresentation.SpriteReference)
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetDurationData(DurationType.Hour, 8)
                .SetTargetingData(Side.Ally, RangeType.Touch, 1, TargetType.IndividualsUnique)
                .SetEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(conditionAlacrity, ConditionForm.ConditionOperation.Add)
                        .Build())
                .Build())
            .SetCastingTime(ActivationTime.Minute1)
            .SetSpellLevel(1)
            .SetVerboseComponent(true)
            .SetSomaticComponent(true)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolDivination)
            .AddToDB();

        return spell;
    }

    private sealed class InitiativeEndListenerGiftOfAlacrity : IInitiativeEndListener
    {
        private readonly FeatureDefinition _featureDefinition;

        public InitiativeEndListenerGiftOfAlacrity(FeatureDefinition featureDefinition)
        {
            _featureDefinition = featureDefinition;
        }

        public IEnumerator OnInitiativeEnded(GameLocationCharacter locationCharacter)
        {
            const string TEXT = "Feedback/&FeatureGiftOfAlacrityLine";

            var gameLocationScreenBattle = Gui.GuiService.GetScreen<GameLocationScreenBattle>();

            if (Gui.Battle == null || gameLocationScreenBattle == null)
            {
                yield break;
            }

            var rulesetCharacter = locationCharacter.RulesetCharacter;
            var roll = rulesetCharacter.RollDie(DieType.D8, RollContext.None, false, AdvantageType.None, out _, out _);

            gameLocationScreenBattle.initiativeTable.ContenderModified(locationCharacter,
                GameLocationBattle.ContenderModificationMode.Remove, false, false);

            locationCharacter.LastInitiative += roll;
            Gui.Battle.initiativeSortedContenders.Sort(Gui.Battle);

            gameLocationScreenBattle.initiativeTable.ContenderModified(locationCharacter,
                GameLocationBattle.ContenderModificationMode.Add, false, false);

            GameConsoleHelper.LogCharacterUsedFeature(
                locationCharacter.RulesetCharacter,
                _featureDefinition,
                TEXT,
                false,
                (ConsoleStyleDuplet.ParameterType.Initiative, roll.ToString()));
        }
    }

    internal static SpellDefinition BuildMagnifyGravity()
    {
        const string NAME = "MagnifyGravity";

        var movementAffinityMagnifyGravity = FeatureDefinitionMovementAffinityBuilder
            .Create($"MovementAffinity{NAME}")
            .SetGuiPresentationNoContent(true)
            .SetBaseSpeedMultiplicativeModifier(0.5f)
            .AddToDB();

        var conditionGravity = ConditionDefinitionBuilder
            .Create(ConditionDefinitions.ConditionEncumbered, "ConditionGravity")
            .SetOrUpdateGuiPresentation(Category.Condition)
            .SetSpecialDuration(DurationType.Round, 1)
            .SetFeatures(movementAffinityMagnifyGravity)
            .AddToDB();

        var spell = SpellDefinitionBuilder
            .Create(NAME)
            .SetGuiPresentation(Category.Spell, Sprites.GetSprite(NAME, Resources.MagnifyGravity, 128))
            .SetEffectDescription(EffectDescriptionBuilder
                .Create()
                .SetTargetingData(Side.All, RangeType.Distance, 12, TargetType.Sphere, 2)
                .SetDurationData(DurationType.Round, 1)
                .SetEffectAdvancement(EffectIncrementMethod.PerAdditionalSlotLevel, additionalDicePerIncrement: 1)
                .SetSavingThrowData(
                    false,
                    AttributeDefinitions.Constitution,
                    true,
                    EffectDifficultyClassComputation.SpellCastingFeature)
                .SetParticleEffectParameters(Shatter.EffectDescription.EffectParticleParameters)
                .AddEffectForms(
                    EffectFormBuilder
                        .Create()
                        .SetDamageForm(DamageTypeForce, 2, DieType.D8)
                        .HasSavingThrow(EffectSavingThrowType.HalfDamage)
                        .Build(),
                    EffectFormBuilder
                        .Create()
                        .SetConditionForm(conditionGravity, ConditionForm.ConditionOperation.Add)
                        .HasSavingThrow(EffectSavingThrowType.Negates)
                        .Build())
                .Build())
            .SetCastingTime(ActivationTime.Action)
            .SetSpellLevel(1)
            .SetVerboseComponent(true)
            .SetSomaticComponent(true)
            .SetSchoolOfMagic(SchoolOfMagicDefinitions.SchoolTransmutation)
            .AddToDB();

        return spell;
    }

    #endregion
}
