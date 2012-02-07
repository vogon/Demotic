using System;

namespace Demotic.Core
{
    /// <summary>
    ///   A script is an atomic unit of self-amendment; each consists of a trigger 
    ///   and an effect.  A script runs whenever the trigger is true, and carries 
    ///   out the actions described by the effect.
    /// </summary>
    public interface IScript
    {
        void BindTo(World world);
        bool IsTriggered(World world);
        void Run(World world);
    }
}
