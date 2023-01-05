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
        ViewCharacterSetItem GetItem(int _CharacterId);
    }
    
    public class ViewCharactersSet : InitBase, IViewCharactersSet
    {
        private readonly IViewCharacterHead01   m_Head01;
        private readonly IViewCharacterHead02   m_Head02;
        private readonly IViewCharacterLegs01   m_Legs01;
        private readonly IViewCharacterLegsFake m_LegsFake;
        private readonly IViewCharacterTail01   m_Tail01;
        private readonly IViewCharacterTailFake m_TailFake;

        public ViewCharactersSet(
            IViewCharacterHead01   _Head01,
            IViewCharacterHead02   _Head02,
            IViewCharacterLegs01   _Legs01,
            IViewCharacterLegsFake _LegsFake,
            IViewCharacterTail01   _Tail01,
            IViewCharacterTailFake _TailFake)
        {
            m_Head01   = _Head01;
            m_Head02   = _Head02;
            m_Legs01   = _Legs01;
            m_LegsFake = _LegsFake;
            m_Tail01   = _Tail01;
            m_TailFake = _TailFake;
        }
        
        
        public ViewCharacterSetItem GetItem(int _CharacterId)
        {
            IViewCharacterHead head;
            IViewCharacterLegs legs;
            IViewCharacterTail tail;
            switch (_CharacterId)
            {
                case 1:
                    (head, legs, tail) = (m_Head01, m_Legs01, m_Tail01);
                    break;
                case 2:
                    (head, legs, tail) = (m_Head02, m_LegsFake, m_Tail01);
                    break;
                default:
                    (head, legs, tail) = (m_Head01, m_Legs01, m_Tail01);
                    break;
            }
            return new ViewCharacterSetItem(head, legs, tail);
        }
    }
}