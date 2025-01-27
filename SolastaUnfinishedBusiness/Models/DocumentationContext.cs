﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;

namespace SolastaUnfinishedBusiness.Models;

internal static class DocumentationContext
{
    internal static void DumpDocumentation()
    {
        if (!Directory.Exists($"{Main.ModFolder}/Documentation"))
        {
            Directory.CreateDirectory($"{Main.ModFolder}/Documentation");
        }

        foreach (var characterFamilyDefinition in DatabaseRepository.GetDatabase<CharacterFamilyDefinition>()
                     .Where(x => x.Name != "Giant_Rugan" && x.Name != "Ooze"))
        {
            if (characterFamilyDefinition.ContentPack == CeContentPackContext.CeContentPack)
            {
                continue;
            }

            DumpMonsters($"SolastaMonsters{characterFamilyDefinition.Name}",
                x => x.CharacterFamily == characterFamilyDefinition.Name && x.DefaultFaction == "HostileMonsters");
        }

        DumpClasses("UnfinishedBusiness", x => x.ContentPack == CeContentPackContext.CeContentPack);
        DumpClasses("Solasta", x => x.ContentPack != CeContentPackContext.CeContentPack);
        DumpSubclasses("UnfinishedBusiness", x => x.ContentPack == CeContentPackContext.CeContentPack);
        DumpSubclasses("Solasta", x => x.ContentPack != CeContentPackContext.CeContentPack);
        DumpRaces("UnfinishedBusiness", x => x.ContentPack == CeContentPackContext.CeContentPack);
        DumpRaces("Solasta", x => x.ContentPack != CeContentPackContext.CeContentPack);
        DumpOthers<FeatDefinition>("UnfinishedBusinessFeats", x => FeatsContext.Feats.Contains(x));
        DumpOthers<FeatDefinition>("SolastaFeats", x => x.ContentPack != CeContentPackContext.CeContentPack);
        DumpOthers<FightingStyleDefinition>("UnfinishedBusinessFightingStyles",
            x => FightingStyleContext.FightingStyles.Contains(x));
        DumpOthers<FightingStyleDefinition>("SolastaFightingStyles",
            x => x.ContentPack != CeContentPackContext.CeContentPack);
        DumpOthers<InvocationDefinition>("UnfinishedBusinessInvocations",
            x => InvocationsContext.Invocations.Contains(x));
        DumpOthers<InvocationDefinition>("SolastaInvocations",
            x => x.ContentPack != CeContentPackContext.CeContentPack);
        DumpOthers<SpellDefinition>("UnfinishedBusinessSpells",
            x => x.ContentPack == CeContentPackContext.CeContentPack && SpellsContext.Spells.Contains(x));
        DumpOthers<SpellDefinition>("SolastaSpells",
            x => x.ContentPack != CeContentPackContext.CeContentPack);
        DumpOthers<ItemDefinition>("UnfinishedBusinessItems",
            x => x.ContentPack == CeContentPackContext.CeContentPack &&
                 x is ItemDefinition item &&
                 (item.IsArmor || item.IsWeapon));
        DumpOthers<ItemDefinition>("SolastaItems",
            x => x.ContentPack != CeContentPackContext.CeContentPack &&
                 x is ItemDefinition item &&
                 (item.IsArmor || item.IsWeapon));
        DumpOthers<MetamagicOptionDefinition>("UnfinishedBusinessMetamagic",
            x => MetamagicContext.Metamagic.Contains(x));
        DumpOthers<MetamagicOptionDefinition>("SolastaMetamagic",
            x => x.ContentPack != CeContentPackContext.CeContentPack);
    }

    private static string LazyManStripXml(string input)
    {
        return input
            .Replace("<color=#add8e6ff>", string.Empty)
            .Replace("<#57BCF4>", "\r\n\t")
            .Replace("<#B5D3DE>", string.Empty)
            .Replace("</color>", string.Empty)
            .Replace("<b>", string.Empty)
            .Replace("<i>", string.Empty)
            .Replace("</b>", string.Empty)
            .Replace("</i>", string.Empty);
    }

    private static void DumpClasses(string groupName, Func<BaseDefinition, bool> filter)
    {
        var outString = new StringBuilder();
        var counter = 1;

        foreach (var klass in DatabaseRepository.GetDatabase<CharacterClassDefinition>()
                     .Where(x => filter(x))
                     .OrderBy(x => x.FormatTitle()))
        {
            outString.AppendLine($"# {counter++}. - {klass.FormatTitle()}");
            outString.AppendLine();
            outString.AppendLine(LazyManStripXml(klass.FormatDescription()));
            outString.AppendLine();

            var level = 0;

            foreach (var featureUnlockByLevel in klass.FeatureUnlocks
                         .Where(x => !x.FeatureDefinition.GuiPresentation.hidden)
                         .OrderBy(x => x.level))
            {
                if (level != featureUnlockByLevel.level)
                {
                    outString.AppendLine();
                    outString.AppendLine($"## Level {featureUnlockByLevel.level}");
                    outString.AppendLine();
                    level = featureUnlockByLevel.level;
                }

                var featureDefinition = featureUnlockByLevel.FeatureDefinition;
                var description = LazyManStripXml(featureDefinition.FormatDescription());

                outString.AppendLine($"* {featureDefinition.FormatTitle()}");
                outString.AppendLine();
                outString.AppendLine(description);
                outString.AppendLine();
            }

            outString.AppendLine();
            outString.AppendLine();
        }

        using var sw = new StreamWriter($"{Main.ModFolder}/Documentation/{groupName}Classes.md");
        sw.WriteLine(outString.ToString());
    }

    private static void DumpSubclasses(string groupName, Func<BaseDefinition, bool> filter)
    {
        var outString = new StringBuilder();
        var counter = 1;

        foreach (var subclass in DatabaseRepository.GetDatabase<CharacterSubclassDefinition>()
                     .Where(x => filter(x))
                     .OrderBy(x => x.FormatTitle()))
        {
            outString.AppendLine($"# {counter++}. - {subclass.FormatTitle()}");
            outString.AppendLine();
            outString.AppendLine(LazyManStripXml(subclass.FormatDescription()));
            outString.AppendLine();

            var level = 0;

            foreach (var featureUnlockByLevel in subclass.FeatureUnlocks
                         .Where(x => !x.FeatureDefinition.GuiPresentation.hidden)
                         .OrderBy(x => x.level))
            {
                if (level != featureUnlockByLevel.level)
                {
                    outString.AppendLine();
                    outString.AppendLine($"## Level {featureUnlockByLevel.level}");
                    outString.AppendLine();
                    level = featureUnlockByLevel.level;
                }

                var featureDefinition = featureUnlockByLevel.FeatureDefinition;
                var description = LazyManStripXml(featureDefinition.FormatDescription());

                outString.AppendLine($"* {featureDefinition.FormatTitle()}");
                outString.AppendLine();
                outString.AppendLine(description);
                outString.AppendLine();
            }

            outString.AppendLine();
            outString.AppendLine();
        }

        using var sw = new StreamWriter($"{Main.ModFolder}/Documentation/{groupName}Subclasses.md");
        sw.WriteLine(outString.ToString());
    }

    private static void DumpRaces(string groupName, Func<BaseDefinition, bool> filter)
    {
        var outString = new StringBuilder();
        var counter = 1;

        foreach (var race in DatabaseRepository.GetDatabase<CharacterRaceDefinition>()
                     .Where(x => filter(x))
                     .OrderBy(x => x.FormatTitle()))
        {
            outString.AppendLine($"# {counter++}. - {race.FormatTitle()}");
            outString.AppendLine();
            outString.AppendLine(LazyManStripXml(race.FormatDescription()));
            outString.AppendLine();

            var level = 0;

            foreach (var featureUnlockByLevel in race.FeatureUnlocks
                         .Where(x => !x.FeatureDefinition.GuiPresentation.hidden)
                         .OrderBy(x => x.level))
            {
                if (level != featureUnlockByLevel.level)
                {
                    outString.AppendLine();
                    outString.AppendLine($"## Level {featureUnlockByLevel.level}");
                    outString.AppendLine();
                    level = featureUnlockByLevel.level;
                }

                var featureDefinition = featureUnlockByLevel.FeatureDefinition;
                var description = LazyManStripXml(featureDefinition.FormatDescription());

                outString.AppendLine($"* {featureDefinition.FormatTitle()}");
                outString.AppendLine();
                outString.AppendLine(description);
                outString.AppendLine();
            }

            outString.AppendLine();
            outString.AppendLine();
        }

        using var sw = new StreamWriter($"{Main.ModFolder}/Documentation/{groupName}Races.md");
        sw.WriteLine(outString.ToString());
    }

    private static void DumpOthers<T>(string groupName, Func<BaseDefinition, bool> filter) where T : BaseDefinition
    {
        var outString = new StringBuilder();
        var db = DatabaseRepository.GetDatabase<T>();
        var counter = 1;

        foreach (var featureDefinition in db
                     .Where(x => filter(x))
                     .OrderBy(x => x.FormatTitle()))
        {
            var description = LazyManStripXml(featureDefinition.FormatDescription());

            outString.AppendLine($"# {counter++}. - {featureDefinition.FormatTitle()}");
            outString.AppendLine();
            outString.AppendLine(description);
            outString.AppendLine();
        }

        using var sw = new StreamWriter($"{Main.ModFolder}/Documentation/{groupName}.md");
        sw.WriteLine(outString.ToString());
    }

    private static void DumpMonsters(string groupName, Func<MonsterDefinition, bool> filter)
    {
        var outString = new StringBuilder();
        var counter = 1;

        foreach (var monsterDefinition in DatabaseRepository.GetDatabase<MonsterDefinition>()
                     .Where(filter)
                     .OrderBy(x => x.FormatTitle()))
        {
            outString.Append(GetMonsterBlock(monsterDefinition, ref counter));
        }

        using var sw = new StreamWriter($"{Main.ModFolder}/Documentation/{groupName}.md");
        sw.WriteLine(outString.ToString());
    }

    private static string GetMonsterBlock([NotNull] MonsterDefinition monsterDefinition, ref int counter)
    {
        var outString = new StringBuilder();

        outString.AppendLine(
            $"# {counter++}. - {monsterDefinition.FormatTitle()}");
        outString.AppendLine();

        var description = LazyManStripXml(monsterDefinition.FormatDescription());

        if (!string.IsNullOrEmpty(description))
        {
            outString.AppendLine(monsterDefinition.FormatDescription());
        }

        outString.AppendLine();
        outString.AppendLine($"Alignment: *{monsterDefinition.Alignment.SplitCamelCase()}* ");

        var inDungeonMaker = monsterDefinition.DungeonMakerPresence == MonsterDefinition.DungeonMaker.None
            ? "NO"
            : "YES";

        outString.AppendLine();
        outString.AppendLine($"Dungeon Maker: *{inDungeonMaker}* ");

        outString.AppendLine();
        outString.AppendLine($"Size: *{monsterDefinition.SizeDefinition.FormatTitle()}* ");

        outString.AppendLine("|  AC  |   HD   |  CR  |");
        outString.AppendLine("| ---- | ------ | ---- |");

        outString.Append($"|  {monsterDefinition.ArmorClass:0#}  ");
        outString.Append($"| {monsterDefinition.HitDice:0#} {monsterDefinition.HitDiceType} ");
        outString.Append($"| {monsterDefinition.ChallengeRating:0#.0} ");
        outString.Append('|');
        outString.AppendLine();

        outString.AppendLine();
        outString.AppendLine("| Str | Dex | Con | Int | Wis | Cha |");
        outString.AppendLine("| --- | --- | --- | --- | --- | --- |");

        outString.Append($"|  {monsterDefinition.AbilityScores[0]:0#} ");
        outString.Append($"|  {monsterDefinition.AbilityScores[1]:0#} ");
        outString.Append($"|  {monsterDefinition.AbilityScores[2]:0#} ");
        outString.Append($"|  {monsterDefinition.AbilityScores[3]:0#} ");
        outString.Append($"|  {monsterDefinition.AbilityScores[4]:0#} ");
        outString.Append($"|  {monsterDefinition.AbilityScores[5]:0#} ");
        outString.Append('|');
        outString.AppendLine();

        outString.AppendLine();
        outString.AppendLine("*Features:*");

        FeatureDefinitionCastSpell featureDefinitionCastSpell = null;

        foreach (var featureDefinition in monsterDefinition.Features
                     .Where(x => x != null)
                     .OrderBy(x => x.Name))
        {
            switch (featureDefinition)
            {
                case FeatureDefinitionCastSpell definitionCastSpell:
                    featureDefinitionCastSpell = definitionCastSpell;
                    break;
                default:
                    outString.Append(GetMonsterFeatureBlock(featureDefinition));
                    break;
            }
        }

        if (featureDefinitionCastSpell != null)
        {
            outString.AppendLine();
            outString.AppendLine("*Spells:*");
            outString.AppendLine("| Level | Spell | Description |");
            outString.AppendLine("| ----- | ----- | ----------- |");

            foreach (var spellsByLevelDuplet in featureDefinitionCastSpell.SpellListDefinition.SpellsByLevel)
            {
                foreach (var spell in spellsByLevelDuplet.Spells)
                {
                    outString.AppendLine(
                        $"| {spellsByLevelDuplet.level} | {spell.FormatTitle()} | {spell.FormatDescription()} |");
                }
            }

            outString.AppendLine();
        }

        outString.AppendLine();
        outString.AppendLine("*Attacks:*");
        outString.AppendLine("| Type | Reach | Hit Bonus | Max Uses |");
        outString.AppendLine("| ---- | ----- | --------- | -------- |");

        foreach (var attackIteration in monsterDefinition.AttackIterations
                     .OrderBy(x => x.MonsterAttackDefinition.Name))
        {
            var title = attackIteration.MonsterAttackDefinition.FormatTitle();

            if (title == "None")
            {
                title = attackIteration.MonsterAttackDefinition.name.SplitCamelCase();
            }

            outString.Append($"| {title} ");
            outString.Append($"| {attackIteration.MonsterAttackDefinition.ReachRange} ");
            outString.Append($"| {attackIteration.MonsterAttackDefinition.ToHitBonus} ");
            outString.Append(attackIteration.MonsterAttackDefinition.MaxUses < 0
                ? "| 1 "
                : $"| {attackIteration.MonsterAttackDefinition.MaxUses} ");
            outString.Append('|');
            outString.AppendLine();
        }

        if (!Main.Settings.EnableBetaContent)
        {
            outString.AppendLine();
            outString.AppendLine();

            return outString.ToString();
        }

        outString.AppendLine();
        outString.AppendLine("*Battle Decisions:*");
        outString.AppendLine("| Name | Weight | Cooldown |");
        outString.AppendLine("| ---- | ------ | -------- |");

        foreach (var weightedDecision in monsterDefinition.DefaultBattleDecisionPackage.Package.WeightedDecisions
                     .OrderBy(x => x.Weight))
        {
            var name = weightedDecision.DecisionDefinition.ToString()
                .Replace("_", string.Empty)
                .Replace("(TA.AI.DecisionDefinition)", string.Empty)
                .SplitCamelCase();

            outString.AppendLine($"| {name} | {weightedDecision.Weight} | {weightedDecision.Cooldown} |");
        }

        outString.AppendLine();
        outString.AppendLine();

        return outString.ToString();
    }

    private static string GetMonsterFeatureBlock(BaseDefinition featureDefinition)
    {
        var outString = new StringBuilder();

        switch (featureDefinition)
        {
            case FeatureDefinitionFeatureSet featureDefinitionFeatureSet:
            {
                foreach (var featureDefinitionFromSet in featureDefinitionFeatureSet.FeatureSet)
                {
                    outString.Append(GetMonsterFeatureBlock(featureDefinitionFromSet));
                }

                break;
            }
            case FeatureDefinitionMoveMode featureDefinitionMoveMode:
                outString.Append("* ");
                outString.Append(featureDefinitionMoveMode.MoveMode);
                outString.Append(' ');
                outString.Append(featureDefinitionMoveMode.Speed);
                outString.AppendLine();

                break;
            case FeatureDefinitionLightAffinity featureDefinitionLightAffinity:
                foreach (var lightingEffectAndCondition in
                         featureDefinitionLightAffinity.LightingEffectAndConditionList)
                {
                    outString.AppendLine(
                        $"* {lightingEffectAndCondition.condition.FormatTitle()} - {lightingEffectAndCondition.lightingState}");
                }

                break;
            default:
                var title = featureDefinition.FormatTitle();

                if (title == "None")
                {
                    title = featureDefinition.Name.SplitCamelCase();
                }

                outString.Append("* ");
                outString.AppendLine(title);

                break;
        }

        return outString.ToString();
    }
}
