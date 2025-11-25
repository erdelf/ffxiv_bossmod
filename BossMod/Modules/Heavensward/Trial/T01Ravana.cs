using BossMod.Components;

namespace BossMod.Heavensward.Trial.T01Ravana;
public enum OID : uint
{
    _Gen_Ravana      = 0x13D2, // R0.500, x9, mixed types
    _Gen_Ravana1     = 0x1201, // R3.500, x7, Helper type
    _Gen_IronGate    = 0x10A1, // R7.000, x8
    _Gen_Ravana2     = 0x1335, // R3.500, x1, Helper type
    Boss             = 0xEB4,  // R3.500, x1
    _Gen_SpiritGana  = 0xEB8,  // R0.600, x0 (spawn during fight)
    _Gen_RavanasWill = 0xEBA,  // R1.000, x0 (spawn during fight), Helper type
}

public enum AID : uint
{
    _AutoAttack_Attack = 5093, // Boss->player, no cast, single-target
    _Weaponskill_BlindingBlade = 3715, // Boss->self, no cast, range 7+R ?-degree cone
    _Weaponskill_TheSeeingTail = 3716, // Boss->self, 1.5s cast, single-target
    _Weaponskill_ScorpionAvatar = 3713, // Boss->self, no cast, single-target
    _Ability_ = 5055, // 13D2->self, no cast, single-target
    _Weaponskill_BladesOfCarnageAndLiberation = 4986, // Boss->self, no cast, single-target
    _Weaponskill_PreludeToSlaughter = 3719, // Boss->self, 15.0s cast, range 15 circle
    _Weaponskill_PreludeToSlaughter1 = 3720, // Boss->self, 3.0s cast, single-target
    _Weaponskill_ = 5052, // 13D2->self, 4.0s cast, range 40+R width 8 rect
    _Weaponskill_PreludeToSlaughter2 = 3721, // 13D2->self, no cast, ???
    _Weaponskill_1 = 5059, // 13D2->self, 3.0s cast, range 20+R circle
    _Weaponskill_PreludeToSlaughter3 = 3722, // Boss->self, 3.0s cast, range 20+R circle
    _Weaponskill_Slaughter = 3723, // Boss->self, 16.0s cast, range 40+R ?-degree cone
    _Weaponskill_Slaughter1 = 3724, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Slaughter2 = 3725, // 13D2->self, 4.0s cast, range 44+R width 8 rect
    _Weaponskill_Slaughter3 = 3726, // 1201->self, no cast, range 12 circle
    _Weaponskill_DragonflyAvatar = 3712, // Boss->self, no cast, single-target
    _Ability_1 = 4761, // 13D2->self, no cast, single-target
    _Weaponskill_2 = 3733, // Boss->self, no cast, single-target
    _Weaponskill_TheSeeingWing = 3717, // Boss->self, 1.5s cast, single-target
    _Spell_Blizzard = 5016, // EB8->player, 1.0s cast, single-target
    _Weaponskill_Tapasya = 3727, // Boss->self, no cast, range 9+R ?-degree cone
    _Weaponskill_Tapasya1 = 3728, // 13D2->self, no cast, range 12+R ?-degree cone
    _Weaponskill_BloodyFuller = 3731, // Boss->self, 5.0s cast, range 100 circle
    _Weaponskill_Chandrahas = 3735, // 1335->self, no cast, range 100 circle
    _Weaponskill_BeetleAvatar = 3714, // Boss->self, no cast, single-target
    _Ability_2 = 5056, // 13D2->self, no cast, single-target
    _Weaponskill_PillarsOfHeaven = 3738, // Boss->self, 3.0s cast, range 40 circle
    _Weaponskill_Surpanakha = 3739, // Boss->self, no cast, range 40+R ?-degree cone
    _Weaponskill_TheRoseOfConviction = 3736, // Boss->self, no cast, single-target
    _Weaponskill_TheRoseOfHate = 3740, // Boss->self, 3.0s cast, range 40+R width 8 rect
    _Weaponskill_TheRoseOfConquest = 3737, // EBA->self, no cast, range 6 circle
    _Weaponskill_SwiftSlaughter = 3741, // Boss->self, 17.0s cast, single-target
    _Weaponskill_FallingLaughter = 3730, // EB8->self, 10.0s cast, single-target
}

public enum IconID : uint
{
    _Gen_Icon_m0244trg_b1t = 57, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_tergetfix1f = 17, // EBA->player
}


class _Weaponskill_PreludeToSlaughter(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_PreludeToSlaughter, 15f);
class _Weaponskill_(BossModule                   module) : Components.StandardAOEs(module, AID._Weaponskill_, new AOEShapeRect(40, 4f, 4f));
class _Weaponskill_1(BossModule                  module) : Components.StandardAOEs(module, AID._Weaponskill_1, new AOEShapeDonut(6f,20f));
class _Weaponskill_Slaughter(BossModule          module) : KnockbackFromCastTarget(module, AID._Weaponskill_Slaughter, 10f);

class _Weaponskill_Slaughter1(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID._Gen_Icon_m0244trg_b1t, AID._Weaponskill_Slaughter3, 12f, 1.280f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == SpreadAction)
        {
            if(Spreads.Any())
            {
                Spreads.RemoveAt(0);
            }

            ++NumFinishedSpreads;
        }
    }
}

class Burn(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Slaughter2, new AOEShapeRect(44, 5f, 4f))
{
    private readonly List<AOEInstance> aoes  = [];
    private readonly List<DateTime>    times = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var instance in base.ActiveAOEs(slot, actor))
        {
            yield return instance;
        }
        foreach (var aoe in aoes)
        {
            yield return aoe;
        }

        if(times.Any() && WorldState.CurrentTime > times.First())
        {
            aoes.RemoveAt(0);
            times.RemoveAt(0);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action == WatchedAction)
        {
            aoes.Add(new AOEInstance(Shape, caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation, WorldState.CurrentTime, Color, Risky));
            times.Add(WorldState.CurrentTime.AddSeconds(5));
        }
    }
}

class _Spell_Blizzard(BossModule              module) : Components.SingleTargetCast(module, AID._Spell_Blizzard);
class _Weaponskill_BloodyFuller(BossModule    module) : Components.RaidwideCast(module, AID._Weaponskill_BloodyFuller);
class _Weaponskill_PillarsOfHeaven(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Weaponskill_PillarsOfHeaven, 40f);
class _Weaponskill_TheRoseOfHate(BossModule   module) : Components.StandardAOEs(module, AID._Weaponskill_TheRoseOfHate, new AOEShapeRect(43, 5f, 4f));
class Adds(BossModule                         module) : Components.Adds(module, (uint) OID._Gen_SpiritGana, 2);

class tether(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCircle(2f), (uint)TetherID._Gen_Tether_chn_tergetfix1f);

class T01RavanaStates : StateMachineBuilder
{
    public T01RavanaStates(BossModule module) : base(module)
    {
        TrivialPhase().
            ActivateOnEnter<_Weaponskill_PreludeToSlaughter>().
            ActivateOnEnter<_Weaponskill_>().
            ActivateOnEnter<_Weaponskill_1>().
            ActivateOnEnter<_Weaponskill_Slaughter>().
            ActivateOnEnter<_Weaponskill_Slaughter1>().
            ActivateOnEnter<Burn>().
            ActivateOnEnter<Adds>().
            ActivateOnEnter<tether>().
            ActivateOnEnter<_Spell_Blizzard>().
            ActivateOnEnter<_Weaponskill_BloodyFuller>().
            ActivateOnEnter<_Weaponskill_PillarsOfHeaven>().
            ActivateOnEnter<_Weaponskill_TheRoseOfHate>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 86, NameID = 3660)]
public class T01Ravana(WorldState ws, Actor primary) : BossModule(ws, primary, new(0f, 0f), new ArenaBoundsCircle(20));
