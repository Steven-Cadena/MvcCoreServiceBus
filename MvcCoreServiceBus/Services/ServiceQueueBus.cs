using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MvcCoreServiceBus.Services
{
    public class ServiceQueueBus
    {
        private ServiceBusClient client;
        //LOS PROCESOS DE RECEPCION DE MENSAJES SE REALIZAN DE FORMA  ASINCRONA
        // Y SE UTILIZA UN METODO DELEGADO QUE IRA LEYENDO CADA MENSAJE

        private List<string> mensajes;
        public ServiceQueueBus(string keys) 
        {
            this.client = new ServiceBusClient(keys);
            this.mensajes = new List<string>();
        }

        //METODO PARA ENVIAR MENSAJES
        public async Task SendMessageAsync(string data) 
        {
            //PARA ENVIAR MENSAJES NECESITAMOS UN SENDER 
            // A PARTIR DE QUEUE
            ServiceBusSender sender = this.client.CreateSender("developers");
            //EL OBJETO PARA ENVIAR MENSAJES ES MESSAGE
            ServiceBusMessage message =
                new ServiceBusMessage(data);
            await sender.SendMessageAsync(message);
        }

        //METODO PARA RECIBIR LOS MENSAJES
        //UTILIZA METODOS DELEGADOS PARA PROCESAR CADA LECTURA DE MENSAJE
        public async Task<List<string>> ReceiveMessagesAsync() 
        {
            ServiceBusProcessor processor =
                this.client.CreateProcessor("developers");
            //EL PROCESO DE LECTURA SE DEBE REALIZAR EN OTROS METODOS
            // ES DECIR, RELLENAR LA LISTA DE MENSAJES SE HACE EN OTRO METODO
            //Y ESTE METODO DEVUELVE LOS MENSAJES
            //DELEGAR PROCESO DE LECTURA 
            processor.ProcessMessageAsync += Processor_ProcessMessageAsync;
            //DELEGAR METODO POR EXCEPCIONES
            processor.ProcessErrorAsync += Processor_ProcessErrorAsync;
            //AQUI COMIENZA A LEER LA COLA DEL MENSAJE
            await processor.StartProcessingAsync();
            //COMO ESTAMOS UTILIZANDO SERVICIOS GRATUITOS ES POSIBLE QUE TARDE EN 
            //LEER Y CONSUMIR
            //DORMIMOS UN POCO LA RESPUESTA PARA NUESTRAS PRUEBAS
            Thread.Sleep(5000);
            //AQUI FINALIZAMOS LA LECTURA DE MENSAJES
            await processor.StopProcessingAsync();
            //DEVOLVEMOS LOS MENSAJES QUE TENGAMOS LEIDOS EN EL METODO
            return this.mensajes;
        }
        private async Task Processor_ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            //AQUI LEEMOS CADA MENSAJE Y DECIDIMOS QUE HACER
            string contenido = arg.Message.Body.ToString();
            //AÑADIMOS LOS MENSAJES A NUESTRA CLASE List
            this.mensajes.Add(contenido);
            //DEBEMOS INDICAR QUE HEMOS PROCESADO ESTE MENSAJE
            await arg.CompleteMessageAsync(arg.Message);
        }

        private Task Processor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            //ENTRA EN CASO QUE DIERA UNA EXCEPCION AL PROCESAR LOS MENSAJES
            Debug.WriteLine(arg.Exception.ToString());
            return Task.CompletedTask;
        }

        
    }
}
