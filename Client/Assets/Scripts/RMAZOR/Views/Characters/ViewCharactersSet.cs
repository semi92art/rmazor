using System.Collections.Generic;
using System.Linq;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using RMAZOR.Views.Characters.Head;
using RMAZOR.Views.Characters.Legs;
using RMAZOR.Views.Characters.Tails;

namespace RMAZOR.Views.Characters
{
    public class ViewCharacterSetItem
    {
        public IViewCharacterHead Head { get; }
        public IViewCharacterLegs Legs { get; }
        public IViewCharacterTail Tail { get; }

        public ViewCharacterSetItem(
            IViewCharacterHead _Head,
            IViewCharacterLegs _Legs, 
            IViewCharacterTail _Tail)
        {
            Head = _Head;
            Legs = _Legs;
            Tail = _Tail;
        }
    }
    
    public interface IViewCharactersSet : IInit
    {
        ViewCharacterSetItem              GetItem(string _CharacterId);
        IEnumerable<ViewCharacterSetItem> GetAllItems();
    }
    
    public class ViewCharactersSet : InitBase, IViewCharactersSet
    {
        #region inject

        private IViewCharacterHead01         Head01         { get; }
        private IViewCharacterHead02         Head02         { get; }
        private IViewCharacterHead03         Head03         { get; }
        private IViewCharacterHead04         Head04         { get; }
        private IViewCharacterHead05         Head05         { get; }
        private IViewCharacterHead06         Head06         { get; }
        private IViewCharacterHeadBatman     HeadBatman     { get; }
        private IViewCharacterHeadBunny      HeadBunny      { get; }
        private IViewCharacterHeadDartVaider HeadDartVaider { get; }
        private IViewCharacterHeadDeadpool   HeadDeadpool   { get; }
        private IViewCharacterHeadDog        HeadDog        { get; }
        private IViewCharacterHeadElephant   HeadElephant   { get; }
        private IViewCharacterHeadFlash      HeadFlash      { get; }
        private IViewCharacterHeadFroggy     HeadFroggy     { get; }
        private IViewCharacterHeadFriday13   HeadFriday13   { get; }
        private IViewCharacterHeadIronMan1   HeadIronMan1   { get; }
        private IViewCharacterHeadIronMan2   HeadIronMan2   { get; }
        private IViewCharacterHeadIronMan3   HeadIronMan3   { get; }
        private IViewCharacterHeadJoker      HeadJoker      { get; }
        private IViewCharacterHeadKitty      HeadKitty      { get; }
        private IViewCharacterHeadSpiderMan  HeadSpiderMan  { get; }
        private IViewCharacterLegs01         Legs01         { get; }
        private IViewCharacterLegsFake       LegsFake       { get; }
        private IViewCharacterTail01         Tail01         { get; }
        private IViewCharacterTailFake       TailFake       { get; }


        private ViewCharactersSet(
            IViewCharacterHead01         _Head01,
            IViewCharacterHead02         _Head02,
            IViewCharacterHead03         _Head03,
            IViewCharacterHead04         _Head04,
            IViewCharacterHead05         _Head05,
            IViewCharacterHead06         _Head06,
            IViewCharacterHeadBatman     _HeadBatman,
            IViewCharacterHeadBunny      _HeadBunny,
            IViewCharacterHeadDartVaider _HeadDartVaider,
            IViewCharacterHeadDeadpool   _HeadDeadpool,
            IViewCharacterHeadDog        _HeadDog,
            IViewCharacterHeadElephant   _HeadElephant,
            IViewCharacterHeadFlash      _HeadFlash,
            IViewCharacterHeadFroggy     _HeadFroggy,
            IViewCharacterHeadFriday13   _HeadFriday13,
            IViewCharacterHeadIronMan1   _HeadIronMan1,
            IViewCharacterHeadIronMan2   _HeadIronMan2,
            IViewCharacterHeadIronMan3   _HeadIronMan3,
            IViewCharacterHeadJoker      _HeadJoker,
            IViewCharacterHeadKitty      _HeadKitty,
            IViewCharacterHeadSpiderMan  _HeadSpiderMan,
            IViewCharacterLegs01         _Legs01,
            IViewCharacterLegsFake       _LegsFake,
            IViewCharacterTail01         _Tail01,
            IViewCharacterTailFake       _TailFake)
        {
            Head01         = _Head01;
            Head02         = _Head02;
            Head03         = _Head03;
            Head04         = _Head04;
            Head05         = _Head05;
            Head06         = _Head06;
            HeadBatman     = _HeadBatman;
            HeadBunny      = _HeadBunny;
            HeadDartVaider = _HeadDartVaider;
            HeadDeadpool   = _HeadDeadpool;
            HeadDog        = _HeadDog;
            HeadElephant   = _HeadElephant;
            HeadFlash      = _HeadFlash;
            HeadFroggy     = _HeadFroggy;
            HeadFriday13   = _HeadFriday13;
            HeadIronMan1   = _HeadIronMan1;
            HeadIronMan2   = _HeadIronMan2;
            HeadIronMan3   = _HeadIronMan3;
            HeadJoker      = _HeadJoker;
            HeadKitty      = _HeadKitty;
            HeadSpiderMan  = _HeadSpiderMan;
            Legs01         = _Legs01;
            LegsFake       = _LegsFake;
            Tail01         = _Tail01;
            TailFake       = _TailFake;
        }

        #endregion

        #region api

        public ViewCharacterSetItem GetItem(string _CharacterId)
        {
            IViewCharacterHead head = GetAllHeads().FirstOrDefault(_Head => _Head.Id == _CharacterId);
            IViewCharacterLegs legs = _CharacterId switch
            {
                "02" => Legs01,
                _    => LegsFake
            };
            IViewCharacterTail tail = Tail01;
            return new ViewCharacterSetItem(head, legs, tail);
        }

        public IEnumerable<ViewCharacterSetItem> GetAllItems()
        {
            return GetAllHeads().Select(_Head => _Head.Id).Select(GetItem);
        }

        #endregion

        #region nonpublic methods

        private IEnumerable<IViewCharacterHead> GetAllHeads()
        {
            return new IViewCharacterHead[]
            {
                Head01,
                Head02,
                Head03,
                Head04,
                Head05,
                Head06,
                HeadBatman,
                HeadBunny,
                HeadDartVaider,
                HeadDeadpool,
                HeadDog,
                HeadElephant,
                HeadFlash,
                HeadFroggy,
                HeadFriday13,
                HeadIronMan1,
                HeadIronMan2,
                HeadIronMan3,
                HeadJoker,
                HeadKitty,
                HeadSpiderMan,
            };
        }

        #endregion
    }
}