using System.Xml.Serialization;
using Backend.Models;

namespace Backend.Services
{
    public class XmlDataService
    {
        private readonly string _dataPath;
        private readonly object _lock = new();

        public XmlDataService(IWebHostEnvironment env)
        {
            _dataPath = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(_dataPath);
            InicializarArchivos();
        }

        private void InicializarArchivos()
        {
            EnsureFile("clientes.xml", new ClienteList());
            EnsureFile("bancos.xml", new BancoList());
            EnsureFile("facturas.xml", new FacturaList());
            EnsureFile("pagos.xml", new PagoList());
        }

        private void EnsureFile<T>(string nombre, T valorDefault)
        {
            var path = Path.Combine(_dataPath, nombre);
            if (!File.Exists(path))
                EscribirXml(path, valorDefault);
        }

        private T LeerXml<T>(string path)
        {
            lock (_lock)
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(T));
                    using var reader = new StreamReader(path);
                    return (T)serializer.Deserialize(reader)!;
                }
                catch
                {
                    // Si el archivo está corrupto, devuelve instancia vacía
                    return Activator.CreateInstance<T>();
                }
            }
        }

        private void EscribirXml<T>(string path, T data)
        {
            lock (_lock)
            {
                var serializer = new XmlSerializer(typeof(T));
                using var writer = new StreamWriter(path);
                serializer.Serialize(writer, data);
            }
        }

        public List<Cliente> GetClientes() =>
            LeerXml<ClienteList>(Path.Combine(_dataPath, "clientes.xml")).Clientes;

        public void SaveClientes(List<Cliente> lista) =>
            EscribirXml(Path.Combine(_dataPath, "clientes.xml"), new ClienteList { Clientes = lista });

        public List<Banco> GetBancos() =>
            LeerXml<BancoList>(Path.Combine(_dataPath, "bancos.xml")).Bancos;

        public void SaveBancos(List<Banco> lista) =>
            EscribirXml(Path.Combine(_dataPath, "bancos.xml"), new BancoList { Bancos = lista });

        public List<Factura> GetFacturas() =>
            LeerXml<FacturaList>(Path.Combine(_dataPath, "facturas.xml")).Facturas;

        public void SaveFacturas(List<Factura> lista) =>
            EscribirXml(Path.Combine(_dataPath, "facturas.xml"), new FacturaList { Facturas = lista });

        public List<Pago> GetPagos() =>
            LeerXml<PagoList>(Path.Combine(_dataPath, "pagos.xml")).Pagos;

        public void SavePagos(List<Pago> lista) =>
            EscribirXml(Path.Combine(_dataPath, "pagos.xml"), new PagoList { Pagos = lista });

        public void LimpiarTodo()
        {
            SaveClientes(new List<Cliente>());
            SaveBancos(new List<Banco>());
            SaveFacturas(new List<Factura>());
            SavePagos(new List<Pago>());
        }
    }
}