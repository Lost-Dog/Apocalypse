using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;

namespace Threepeat
{
    [Title("Is MMLC Enabled")]
    [Description("Returns true if the Character's MMLC is enabled")]

    [Category("MMLC/Is MMLC Enabled")]

    [Serializable]
    public class ConditionIsMMLCEnabled : TConditionCharacter
    {
        protected override bool Run(Args args)
        {
            Character character = this.m_Character.Get<Character>(args);
            MMCGameCreator2 mmcgc = character?.GetComponent<MMCGameCreator2>();
            if (mmcgc == null)
            {
                return false;
            }
            return mmcgc.MMLCCurrentlyEnabled;
        }
    }
}