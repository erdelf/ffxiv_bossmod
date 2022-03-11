﻿using System;
using System.Numerics;

namespace BossMod.Endwalker.ARanks.Yilan
{
    public enum OID : uint
    {
        Boss = 0x35BF,
    };

    public enum AID : uint
    {
        AutoAttack = 872,
        Soundstorm = 27230,
        MiniLight = 27231,
        Devour = 27232, // cone that kills seduced and deals very small damage otherwise, so we ignore...
        BogBomb = 27233,
        BrackishRain = 27234,
    }

    public enum SID : uint
    {
        None = 0,
        ForwardMarch = 1958,
        AboutFace = 1959,
        LeftFace = 1960,
        RightFace = 1961,
    }

    public class Mechanics : BossModule.Component
    {
        private BossModule _module;
        private AOEShapeCircle _miniLight = new(18);
        private AOEShapeCircle _bogBomb = new(6);
        private AOEShapeCone _brackishRain = new(10, MathF.PI / 4); // TODO: verify angle

        private static float _marchDistance = 12;

        public Mechanics(BossModule module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            var (aoe, pos) = ActiveAOE(actor);
            if (aoe?.Check(actor.Position, pos, _module.PrimaryActor.Rotation) ?? false)
                hints.Add("GTFO from aoe!");
        }

        public override void AddGlobalHints(BossModule.GlobalHints hints)
        {
            if (!(_module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return;

            string hint = (AID)_module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.Soundstorm => "Apply march debuffs",
                AID.MiniLight or AID.BogBomb or AID.BrackishRain => "Avoidable AOE",
                AID.Devour => "Harmless unless failed",
                _ => "",
            };
            if (hint.Length > 0)
                hints.Add(hint);
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            var (aoe, pos) = ActiveAOE(pc);
            aoe?.Draw(arena, pos, _module.PrimaryActor.Rotation);
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
        {
            var marchDirection = MarchDirection(pc);
            if (marchDirection != SID.None)
            {
                float dir = marchDirection switch
                {
                    SID.AboutFace => MathF.PI,
                    SID.LeftFace => MathF.PI / 2,
                    SID.RightFace => -MathF.PI / 2,
                    _ => 0
                };
                var target = pc.Position + _marchDistance * GeometryUtils.DirectionToVec3(pc.Rotation + dir);
                arena.AddLine(pc.Position, target, arena.ColorDanger);
                arena.Actor(target, pc.Rotation, arena.ColorDanger);
            }
        }

        private (AOEShape?, Vector3) ActiveAOE(Actor pc)
        {
            if (MarchDirection(pc) != SID.None)
                return (_miniLight, _module.PrimaryActor.Position);

            if (!(_module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return (null, new());

            return (AID)_module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.MiniLight => (_miniLight, _module.PrimaryActor.Position),
                AID.BogBomb => (_bogBomb, _module.PrimaryActor.CastInfo.Location),
                AID.BrackishRain => (_brackishRain, _module.PrimaryActor.Position),
                _ => (null, new())
            };
        }

        private SID MarchDirection(Actor actor)
        {
            foreach (var s in actor.Statuses)
                if ((SID)s.ID is SID.ForwardMarch or SID.AboutFace or SID.LeftFace or SID.RightFace)
                    return (SID)s.ID;
            return SID.None;
        }
    }

    public class Yilan : SimpleBossModule
    {
        public Yilan(BossModuleManager manager, Actor primary)
            : base(manager, primary)
        {
            InitialState?.Enter.Add(() => ActivateComponent(new Mechanics(this)));
        }
    }
}
