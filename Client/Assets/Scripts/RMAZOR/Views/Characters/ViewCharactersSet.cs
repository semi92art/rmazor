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
        ViewCharacterSetItem              GetItem(int _CharacterId);
        IEnumerable<ViewCharacterSetItem> GetAllItems();
    }
    
    public class ViewCharactersSet : InitBase, IViewCharactersSet
    {
        #region inject

        private IViewCharacterHead01   Head01   { get; }
        private IViewCharacterHead02   Head02   { get; }
        private IViewCharacterHead03   Head03   { get; }
        private IViewCharacterHead04   Head04   { get; }
        private IViewCharacterHead05   Head05   { get; }
        private IViewCharacterLegs01   Legs01   { get; }
        private IViewCharacterLegsFake LegsFake { get; }
        private IViewCharacterTail01   Tail01   { get; }
        private IViewCharacterTailFake TailFake { get; }


        private ViewCharactersSet(
            IViewCharacterHead01   _Head01,
            IViewCharacterHead02   _Head02,
            IViewCharacterHead03   _Head03,
            IViewCharacterHead04   _Head04,
            IViewCharacterHead05   _Head05,
            IViewCharacterLegs01   _Legs01,
            IViewCharacterLegsFake _LegsFake,
            IViewCharacterTail01   _Tail01,
            IViewCharacterTailFake _TailFake)
        {
            Head01   = _Head01;
            Head02   = _Head02;
            Head03   = _Head03;
            Head04   = _Head04;
            Head05   = _Head05;
            Legs01   = _Legs01;
            LegsFake = _LegsFake;
            Tail01   = _Tail01;
            TailFake = _TailFake;
        }

        #endregion

        #region api

        public ViewCharacterSetItem GetItem(int _CharacterId)
        {
            IViewCharacterHead head;
            IViewCharacterLegs legs;
            IViewCharacterTail tail;
            switch (_CharacterId)
            {
                case 1:  (head, legs, tail) = (Head01, Legs01,   Tail01); break;
                case 2:  (head, legs, tail) = (Head02, Legs01,   Tail01); break;
                case 3:  (head, legs, tail) = (Head03, LegsFake, Tail01); break;
                case 4:  (head, legs, tail) = (Head04, LegsFake, Tail01); break;
                case 5:  (head, legs, tail) = (Head05, LegsFake, Tail01); break;
                default: (head, legs, tail) = (Head01, Legs01,   Tail01); break;
            }
            return new ViewCharacterSetItem(head, legs, tail);
        }

        public IEnumerable<ViewCharacterSetItem> GetAllItems()
        {
            return Enumerable.Range(1, 5).Select(GetItem);
        }

        #endregion
    }
}