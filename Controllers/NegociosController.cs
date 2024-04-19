using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using appCoreMVC_13.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace appCoreMVC_13.Controllers
{
    public class NegociosController : Controller
    {
        async Task<List<Pais>> getpaises()
        {
            List<Pais> temporal = new List<Pais>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7074/api/apiNegocios/");
                HttpResponseMessage mensaje = await client.GetAsync("paises");
                string cadena = await mensaje.Content.ReadAsStringAsync();

                temporal = JsonConvert.DeserializeObject<List<Pais>>(cadena).Select(
                    s => new Pais
                    {
                        idpais = s.idpais,
                        nombrepais = s.nombrepais

                    }).ToList();
            }
            return temporal;
        }

        async Task<List<Cliente>> getclientes()
        {
            List<Cliente> temporal = new List<Cliente>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:7074/api/apiNegocios/");
                HttpResponseMessage mensaje = await client.GetAsync("clientes");
                string cadena = await mensaje.Content.ReadAsStringAsync();

                temporal = JsonConvert.DeserializeObject<List<Cliente>>(cadena).Select(
                    c => new Cliente
                    {
                        idcliente = c.idcliente,
                        nombrecia = c.nombrecia,
                        direccion = c.direccion,
                        idpais = c.idpais,
                        telefono = c.telefono
                    }).ToList();
            }
            return temporal;
        }

        public async Task<IActionResult> Index()
        {
            //ejecutando
            return View(await getclientes());
        }

        public async Task<IActionResult> Create()
        {
            //LISTA DE PAISES
            ViewBag.paises = new SelectList(await getpaises(), "idpais", "nombrepais");
            return View(await Task.Run(() => new Cliente()));
        }

        [HttpPost]
        public async Task<IActionResult> Create(Cliente reg)
        {
            string mensaje = "";
            using (var cliente = new HttpClient())
            {
                cliente.BaseAddress = new Uri("https://localhost:7074/api/apiNegocios/");
                //DEFINI UN STRING CONTECT PARA ENVIAR REGISTRO EN FORMATO JSON
                StringContent contenidoJson =
                    new StringContent(JsonConvert.SerializeObject(reg), System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage msg = await cliente.PostAsync("agregar", contenidoJson);
                mensaje = await msg.Content.ReadAsStringAsync();
            }

            //REFRESCAR
            ViewBag.mensaje = mensaje;
            ViewBag.paises = new SelectList(await getpaises(), "idpais", "nombrepais", reg.idpais);
            return View(await Task.Run(() => reg));
        }


        public async Task<IActionResult> Actualizar(string id)
        {
            // Buscar el cliente por su id
            Cliente cliente = await buscarCliente(id);

            // Si el cliente no se encuentra, redirigir a alguna página de error o manejar la situación de manera adecuada
            if (cliente == null)
            {
                return NotFound(); // EN CASO DE ERROR
            }

            // PASO EL CLIENTE ENCONTRADO A LA VISTA
            ViewBag.paises = new SelectList(await getpaises(), "idpais", "nombrepais", cliente.idpais);
            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Actualizar(string id, Cliente reg)
        {
            if (id != reg.idcliente)
            {
                return BadRequest(); //SI EL ID NO COINCIDE SE MANDA A UNA PAGINA DE ERROR
            }

            string mensaje = "";
            using (var clienteActualiza = new HttpClient())
            {
                clienteActualiza.BaseAddress = new Uri("https://localhost:7074/api/apiNegocios/");

                StringContent contenidoJsonActualiza =
                    new StringContent(JsonConvert.SerializeObject(reg), System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage msg = await clienteActualiza.PutAsync($"actualizar/{id}", contenidoJsonActualiza);
                mensaje = await msg.Content.ReadAsStringAsync();
            }

            //REFRESCAR
            ViewBag.mensaje = mensaje;
            ViewBag.paises = new SelectList(await getpaises(), "idpais", "nombrepais", reg.idpais);
            return RedirectToAction("Actualizar", new { id = reg.idcliente });
        }


        private async Task<Cliente> buscarCliente(string id)
        {
            using (var cliente = new HttpClient())
            {
                cliente.BaseAddress = new Uri("https://localhost:7074/api/apiNegocios/");
                HttpResponseMessage mensaje = await cliente.GetAsync($"buscar/{id}");
                if (mensaje.IsSuccessStatusCode)
                {
                    string json = await mensaje.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Cliente>(json);
                }
                else
                {
                    return null;
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(string id)
        {
            using (var cliente = new HttpClient())
            {
                cliente.BaseAddress = new Uri("https://localhost:7074/api/apiNegocios/");

                HttpResponseMessage response = await cliente.DeleteAsync($"eliminar/{Uri.EscapeDataString(id)}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cliente eliminado correctamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Ocurrió un error al eliminar el cliente.";
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}