//using System.Collections.Concurrent;
//using Jay.Comparison;
//using Jayflect.Building.Adaption;
//using Jayflect.Building.Emission;

//namespace Jayflect.Fulfilling;

//internal sealed class Proxy<TIn, TOut>
//{
//    private readonly Func<TIn, TOut> _constructor;
        
//    public Proxy(Type constructedProxyType)
//    {
//        if (constructedProxyType is null)
//            throw new ArgumentNullException(nameof(constructedProxyType));
//        var ctor = constructedProxyType.GetConstructor(Reflect.Flags.Instance,
//                                                       null,
//                                                       CallingConventions.Any,
//                                                       new Type[1] { typeof(TIn) }, 
//                                                       null);
//        RuntimeMethodAdapter.TryAdapt(ctor, out _constructor).ThrowIfFailed();
//    }

//    public TOut Construct(TIn value) => _constructor(value);
//}
    
///// <summary>
///// https://weblogs.asp.net/seanmcalinden/creating-a-dynamic-proxy-generator-part-1-creating-the-assembly-builder-module-builder-and-caching-mechanism
///// https://weblogs.asp.net/seanmcalinden/creating-a-dynamic-proxy-generator-with-csharp-part-2-interceptor-design
///// </summary>
//public static class DynamicProxyBuilder
//{
//    private static readonly AssemblyBuilder _assemblyBuilder;
//    private static readonly ModuleBuilder _moduleBuilder;
//    private static readonly IComparer<MemberInfo> _memberComparer = new FuncComparer<MemberInfo>((x,y) => Comparer<MemberTypes>.Default.Compare(x?.MemberType ?? default, y?.MemberType ?? default));
        
//    private static readonly ConcurrentDictionary<(Type SourceType, Type DestType), object> _proxyCache;
        
//    static DynamicProxyBuilder()
//    {
//        var assemblyName = new AssemblyName("DynamicProxyBuilderAssembly");
//        _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
//        var moduleName = "DynamicProxyBuilderModule";
//        _moduleBuilder = _assemblyBuilder.DefineDynamicModule(moduleName);
//        _proxyCache = new ConcurrentDictionary<(Type, Type), object>(Environment.ProcessorCount * 2, 0);
//    }

//    public static TOut ProxyAs<TIn, TOut>(TIn value)
//    {
//        if (value is TOut alreadyTyped)
//            return alreadyTyped;
//        var key = (typeof(TIn), typeof(TOut));
//        var data = _proxyCache.GetOrAdd(key, CreateAdapter);
//        if (data is Type adapterType)
//        {
//            var proxy = new Proxy<TIn, TOut>(adapterType);
//            _proxyCache[key] = proxy;
//            return proxy.Construct(value);
//        }
//        else if (data is Proxy<TIn, TOut> proxy)
//        {
//            return proxy.Construct(value);
//        }
//        else
//        {
//            throw new InvalidOperationException();
//        }
//    }

//    /*/// <summary>
//    /// Proxies a <see langword="static"/> <see langword="class"/> or <see langword="struct"/> as an interface.
//    /// </summary>
//    /// <typeparam name="TOut">The interface type to proxy as.</typeparam>
//    /// <paramref name="staticType"/>The static type to proxy.
//    /// <returns>A full realized type instance that marshals the <typeparamref name="TOut"/> interface's calls to the underlying static type.</returns>
//    public static TOut ProxyStaticAs<TOut>(Type staticType)
//        where TOut : class
//    {
//        if (staticType is null)
//            throw new ArgumentNullException(nameof(staticType));
//        if (!staticType.IsStatic())
//            throw new ArgumentException("The specified type must be static", nameof(staticType));
//        var key = (staticType, typeof(TOut));
//        var data = _proxyCache.GetOrAdd(key, CreateAdapter);
//        if (data is Type adapterType)
//        {
//            var proxy = new Proxy<TIn, TOut>(adapterType);
//            _proxyCache[key] = proxy;
//            return proxy.Construct(value);
//        }
//        else if (data is Proxy<TIn, TOut> proxy)
//        {
//            return proxy.Construct(value);
//        }
//        else
//        {
//            throw new InvalidOperationException();
//        }
//    }*/
        

//    private static object CreateAdapter((Type SourceType, Type DestType) types)
//    {
//        (Type sourceType, Type destType) = types;
//        if (destType is null)
//            throw new ArgumentNullException(nameof(destType));
            
//        BindingFlags SEARCH_FLAGS;
            
//        if (destType.IsInterface)
//        {
//            var implementedMembers = new HashSet<MemberInfo>(0);
//            Dictionary<MemberInfo, object> builders = new Dictionary<MemberInfo, object>(0);
                
//            SEARCH_FLAGS = BindingFlags.Public | 
//                           BindingFlags.NonPublic | 
//                           BindingFlags.Instance |
//                           BindingFlags.IgnoreCase;

//            // Create our type
//            var adapterTypeName = $"{sourceType.Name}_to_{destType.Name}_adapter";
//            var typeBuilder = _moduleBuilder.DefineType(adapterTypeName, TypeAttributes.Public, destType);
//            // It implements the interface
//            typeBuilder.AddInterfaceImplementation(destType);
                
//            // Store the TIn value
//            var baseField = typeBuilder.DefineField("_base", 
//                                                    sourceType, 
//                                                    FieldAttributes.Private | FieldAttributes.InitOnly);
                
//            // Create our constructor
//            ConstructorBuilder constructor = typeBuilder.DefineConstructor(MethodAttributes.Public,
//                                                                           CallingConventions.Standard,
//                                                                           new Type[1] {sourceType});
//            var ctorEmitter = constructor.GetILEmitter();
//            // Store value in storage
//            ctorEmitter.Ldarg(0)    // this
//                       .Ldarg(1)    // value
//                       .Stfld(baseField)    // _base = value
//                       .Ret();

//            void ImplementMethod(MethodBase method)
//            {
//                // Do not re-implement
//                if (implementedMembers.Contains(method)) return;

//                var data = DelegateInfo.For(method);
                    
//                // Find the method on the source type that we're pulling from
//                var sourceMethod = baseField.FieldType.GetMethod(method.Name, 
//                                                                 SEARCH_FLAGS, 
//                                                                 null, 
//                                                                 data.ParameterTypes, 
//                                                                 null);
//                if (sourceMethod is null)
//                    throw new InvalidOperationException($"Cannot find source method for {method}");
//                var mAttr = MethodAttributes.Public | 
//                            MethodAttributes.Virtual | 
//                            MethodAttributes.HideBySig |
//                            MethodAttributes.Final;

//                var methodBuilder = typeBuilder.DefineMethod(method.Name,
//                                                             mAttr,
//                                                             data.ReturnType,
//                                                             data.ParameterTypes);
//                var emitter = methodBuilder.GetILEmitter();
//                emitter.Ldarg(0)                // this
//                       .Ldfld(baseField);       // ._base
//                // Load method parameters (i == 0 is `this`)
//                for (var i = 1; i <= data.ParameterCount; i++)
//                {
//                    emitter.Ldarg(i);
//                }
//                // Call the source method
//                emitter.Call(sourceMethod)
//                       .Ret();
                    
//                // Cache
//                builders![method] = methodBuilder;
//            }

//            void ImplementProperty(PropertyInfo property)
//            {
//                // Do not re-implement
//                if (implementedMembers.Contains(property)) return;

//                // Build the property
//                var propertyBuilder = typeBuilder.DefineProperty(property.Name,
//                                                                 PropertyAttributes.None,
//                                                                 property.PropertyType,
//                                                                 null);
                    
//                // Implement Get method
//                var getMethod = property.GetGetMethod();
//                if (getMethod != null)
//                {
//                    // We already implemented this Method
//                    var getBuilder = builders[getMethod] as MethodBuilder;
//                    propertyBuilder.SetGetMethod(getBuilder!);
//                }
                    
//                // Implement Set method
//                var setMethod = property.GetSetMethod();
//                if (setMethod != null)
//                {
//                    // We already implemented this Method
//                    var setBuilder = builders[setMethod] as MethodBuilder;
//                    propertyBuilder.SetSetMethod(setBuilder!);
//                }

//                // Cache
//                builders[property] = propertyBuilder;
//            }

//            void ImplementEvent(EventInfo @event)
//            {
//                // Do not re-implement
//                if (implementedMembers.Contains(@event)) return;

//                // Build the event
//                var eventBuilder = typeBuilder.DefineEvent(@event.Name,
//                                                           EventAttributes.None,
//                                                           @event.EventHandlerType!);
                    
//                // Implement Add method
//                var addMethod = @event.GetAddMethod();
//                if (addMethod != null)
//                {
//                    // We already implemented this Method
//                    var addBuilder = builders[addMethod] as MethodBuilder;
//                    eventBuilder.SetAddOnMethod(addBuilder!);
//                }
                    
//                // Implement Remove method
//                var removeMethod = @event.GetRemoveMethod();
//                if (removeMethod != null)
//                {
//                    // We already implemented this Method
//                    var removeBuilder = builders[removeMethod] as MethodBuilder;
//                    eventBuilder.SetRemoveOnMethod(removeBuilder!);
//                }
                    
//                // TODO: Implement Raise Method
//                var raiseMethod = @event.RaiseMethod;
//                if (raiseMethod != null)
//                {
//                    // We've already implemented this Method
//                    var raiseBuilder = builders[raiseMethod] as MethodBuilder;
//                    eventBuilder.SetRaiseMethod(raiseBuilder!);
//                }

//                // Cache
//                builders[@event] = eventBuilder;
//            }
                
//            void ImplementMembers(Type type)
//            {
//                // Do not re-implement
//                if (implementedMembers.Contains(type)) return;
                    
//                // All interface, always
//                foreach (var face in type.GetInterfaces())
//                    ImplementMembers(face);
//                // All public instance members
//                foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
//                                           .OrderBy(m => m, _memberComparer))
//                {
//                    if (member is MethodBase method)
//                        ImplementMethod(method);
//                    else if (member is PropertyInfo property)
//                        ImplementProperty(property);
//                    else if (member is EventInfo @event)
//                        ImplementEvent(@event);
//                    // We implemented this member
//                    implementedMembers.Add(member);
//                }
//                // We implemented this type
//                implementedMembers.Add(type);
//            }
                
//            // Implement everything
//            ImplementMembers(destType);

//            return typeBuilder.CreateType()!;
//        }
//        else if (destType.IsClass)
//        {
//            // All the members have to be abstract / virtual
//        }
            
//        throw new NotImplementedException();
//    }
//}