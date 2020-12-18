using System;
using SDX.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;

namespace _7D2D_Mods.Grow_Lights.Harmony
{
    class GrowLightPatchScript : IPatcherMod
    {
        public bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
        {
            return true;
        }

        public bool Patch(ModuleDefinition module)
        {
            throw new NotImplementedException();
        }
    }
}
