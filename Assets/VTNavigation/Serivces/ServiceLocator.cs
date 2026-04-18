using System;
using System.Collections.Generic;

namespace VTNavigation.Serivces
{
    public class ServiceLocator
    {
        private static Lazy<ServiceLocator> s_Instance = new  Lazy<ServiceLocator>(() => new ServiceLocator());
        
        public static ServiceLocator Instance => s_Instance.Value;
        
        private Dictionary<Type, IService> m_Services = new Dictionary<Type, IService>();

        public void AddService<T>(IService service) where T:IService
        {
            if (service == null || m_Services.ContainsKey(service.ServiceType))
            {
                return;
            }
            m_Services.Add(typeof(T), service);
        }

        public T GetService<T>() where T : IService
        {
            if (m_Services.TryGetValue(typeof(T), out IService service))
            {
                return (T)service;
            }

            return default(T);
        }
    }
}