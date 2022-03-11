﻿using System;

namespace BossMod.Endwalker.ARanks.Gurangatch
{
    public enum OID : uint
    {
        Boss = 0x361B,
    };

    public enum AID : uint
    {
        AutoAttack = 870,
        LeftHammerSlammer = 27493,
        RightHammerSlammer = 27494,
        LeftHammerSecond = 27495,
        RightHammerSecond = 27496,
        OctupleSlammerLCW = 27497, // TODO this is a guess; never seen
        OctupleSlammerRCW = 27498, // TODO this is a guess; never seen
        OctupleSlammerRestL = 27499,
        OctupleSlammerRestR = 27500,
        // WildCharge = 27511? TODO never seen...
        BoneShaker = 27512,
        OctupleSlammerLCCW = 27521,
        OctupleSlammerRCCW = 27522,
    }

    // TODO: safe quarter for octo slammer, verify CW rotation...
    public class Mechanics : BossModule.Component
    {
        private BossModule _module;
        private AOEShapeCone _slammer = new(30, MathF.PI / 2);
        private int _remainingSlams = 0;
        private float _slamDir;
        private float _slamDirIncrement;

        public Mechanics(BossModule module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_remainingSlams > 0 && _slammer.Check(actor.Position, _module.PrimaryActor.Position, _slamDir))
                hints.Add("GTFO from aoe!");
        }

        public override void AddGlobalHints(BossModule.GlobalHints hints)
        {
            if (!(_module.PrimaryActor.CastInfo?.IsSpell() ?? false))
                return;

            string hint = (AID)_module.PrimaryActor.CastInfo.Action.ID switch
            {
                AID.BoneShaker => "Raidwide",
                _ => "",
            };
            if (hint.Length > 0)
                hints.Add(hint);
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            if (_remainingSlams > 0)
                _slammer.Draw(arena, _module.PrimaryActor.Position, _slamDir);
        }

        public override void OnCastStarted(Actor actor)
        {
            if (actor != _module.PrimaryActor || !actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo.Action.ID)
            {
                case AID.LeftHammerSlammer:
                    _remainingSlams = 2;
                    _slamDir = _module.PrimaryActor.Rotation + MathF.PI / 2;
                    _slamDirIncrement = MathF.PI;
                    break;
                case AID.RightHammerSlammer:
                    _remainingSlams = 2;
                    _slamDir = _module.PrimaryActor.Rotation - MathF.PI / 2;
                    _slamDirIncrement = MathF.PI;
                    break;
                case AID.OctupleSlammerLCW:
                    _remainingSlams = 8;
                    _slamDir = _module.PrimaryActor.Rotation + MathF.PI / 2;
                    _slamDirIncrement = MathF.PI / 2;
                    break;
                case AID.OctupleSlammerRCW:
                    _remainingSlams = 8;
                    _slamDir = _module.PrimaryActor.Rotation - MathF.PI / 2;
                    _slamDirIncrement = MathF.PI / 2;
                    break;
                case AID.OctupleSlammerLCCW:
                    _remainingSlams = 8;
                    _slamDir = _module.PrimaryActor.Rotation + MathF.PI / 2;
                    _slamDirIncrement = -MathF.PI / 2;
                    break;
                case AID.OctupleSlammerRCCW:
                    _remainingSlams = 8;
                    _slamDir = _module.PrimaryActor.Rotation - MathF.PI / 2;
                    _slamDirIncrement = -MathF.PI / 2;
                    break;
            }
        }

        public override void OnCastFinished(Actor actor)
        {
            if (actor != _module.PrimaryActor || !actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo.Action.ID)
            {
                case AID.LeftHammerSlammer:
                case AID.RightHammerSlammer:
                case AID.LeftHammerSecond:
                case AID.RightHammerSecond:
                case AID.OctupleSlammerLCW:
                case AID.OctupleSlammerRCW:
                case AID.OctupleSlammerRestL:
                case AID.OctupleSlammerRestR:
                case AID.OctupleSlammerLCCW:
                case AID.OctupleSlammerRCCW:
                    _slamDir += _slamDirIncrement;
                    --_remainingSlams;
                    break;
            }
        }
    }

    public class Gurangatch : SimpleBossModule
    {
        public Gurangatch(BossModuleManager manager, Actor primary)
            : base(manager, primary)
        {
            InitialState?.Enter.Add(() => ActivateComponent(new Mechanics(this)));
        }
    }
}
