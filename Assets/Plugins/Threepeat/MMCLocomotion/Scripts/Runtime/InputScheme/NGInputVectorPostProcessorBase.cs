using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threepeat
{

    public abstract class NGInputVectorPostProcessorBase : ScriptableObject
    {
        [Header("General Settings")]
        public bool postProcessorActive = true;

        [HideInInspector]
        public bool wasPostProcessorActive = false;

        public abstract void PostProcessInputVector(NGInputSchemeBase inputScheme, ref Vector3 inputVectorLocalSpace);

        public virtual void Activate(NGInputSchemeBase inputScheme) {  }

        public virtual void Deactivate(NGInputSchemeBase inputScheme) { }

    } // class

} // namespace
