using Frontend.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Frontend.Pages
{
	public class EstadoCuentaModel : PageModel
	{
		private readonly ApiService _api;
		public EstadoCuentaModel(ApiService api) => _api = api;

		public List<ClienteVM>? Clientes { get; set; }
		public string NitBuscado { get; set; } = "";
		public bool Buscado { get; set; }

		public async Task OnGetAsync(string? nit)
		{
			if (Request.Query.ContainsKey("nit") || !string.IsNullOrWhiteSpace(nit))
			{
				Buscado = true;
				NitBuscado = nit ?? "";
				var json = await _api.GetEstadoCuenta(nit);
				var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
				Clientes = JsonSerializer.Deserialize<List<ClienteVM>>(json, opts) ?? new();
			}
		}
	}

	public class ClienteVM
	{
		public string NIT { get; set; } = "";
		public string Nombre { get; set; } = "";
		public double SaldoActual { get; set; }
		public double SaldoAFavor { get; set; }
		public List<TransaccionVM> Transacciones { get; set; } = new();
	}

	public class TransaccionVM
	{
		public string Fecha { get; set; } = "";
		public string Tipo { get; set; } = "";
		public string Descripcion { get; set; } = "";
		public double Monto { get; set; }
	}
}