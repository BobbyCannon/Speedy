﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Speed.Benchmarks.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Speed.Benchmarks.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to EXEC sp_MSForEachTable &apos;ALTER TABLE ? NOCHECK CONSTRAINT ALL&apos;
        ///EXEC sp_MSForEachTable &apos;ALTER TABLE ? DISABLE TRIGGER ALL&apos;
        ///EXEC sp_MSForEachTable &apos;IF &apos;&apos;?&apos;&apos; NOT LIKE &apos;&apos;%MigrationHistory%&apos;&apos; DELETE FROM ?&apos;
        ///EXEC sp_MSforeachtable &apos;ALTER TABLE ? ENABLE TRIGGER ALL&apos;
        ///EXEC sp_MSForEachTable &apos;ALTER TABLE ? CHECK CONSTRAINT ALL&apos;
        ///EXEC sp_MSForEachTable &apos;IF OBJECTPROPERTY(object_id(&apos;&apos;?&apos;&apos;), &apos;&apos;TableHasIdentity&apos;&apos;) = 1 DBCC CHECKIDENT (&apos;&apos;?&apos;&apos;, RESEED, 0)&apos;.
        /// </summary>
        internal static string ClearDatabase {
            get {
                return ResourceManager.GetString("ClearDatabase", resourceCulture);
            }
        }
    }
}
