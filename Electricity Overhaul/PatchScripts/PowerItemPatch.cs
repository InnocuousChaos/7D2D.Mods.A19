using Mono.Cecil;
using SDX.Compiler;
using System;
using System.Collections.Generic;
using System.Linq;

public class PowerItemPatch : IPatcherMod
{
    public bool Patch(ModuleDefinition module)
    {
        Console.WriteLine("== PowerItemPatch===");
        (from t in module.Types
                from f in t.Fields
                where f.FieldType == module.Import(typeof(PowerItem))
                select f).ToList().ForEach(f => f.FieldType = module.Import(typeof(List<PowerItem>)));
        TypeDefinition gm = module.Types.First(d => d.Name == "PowerItem");
        FieldDefinition myTypeRef = gm.Fields.First(d => d.Name == "Parent");
        myTypeRef.FieldType = module.Import(typeof(List<PowerItem>));

        return true;
    }


    // Called after the patching process and after scripts are compiled.
    // Used to link references between both assemblies
    // Return true if successful
    public bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
    {
        return true;
    }


    // Helper functions to allow us to access and change variables that are otherwise unavailable.
    private void SetMethodToVirtual(MethodDefinition meth)
    {
        meth.IsVirtual = true;
    }

    private void SetFieldToPublic(FieldDefinition field)
    {
        field.IsFamily = false;
        field.IsPrivate = false;
        field.IsPublic = true;

    }
    private void SetMethodToPublic(MethodDefinition field)
    {
        field.IsFamily = false;
        field.IsPrivate = false;
        field.IsPublic = true;

    }
}