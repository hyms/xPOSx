using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace XPos.Domain.Services;

public class SiatSoapService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SiatSoapService> _logger;

    public SiatSoapService(HttpClient httpClient, ILogger<SiatSoapService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> SendSiatSoapRequestAsync(string url, string soapAction, string soapEnvelopeXml, int timeoutSeconds = 5)
    {
        var stopwatch = Stopwatch.StartNew();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("SOAPAction", soapAction);
            request.Content = new StringContent(soapEnvelopeXml, Encoding.UTF8, "text/xml");

            _logger.LogInformation("Iniciando consumo de Web Service SOAP de prueba de SIAT Bolivia en {Url}.", url);

            using var response = await _httpClient.SendAsync(request, cts.Token);
            stopwatch.Stop();

            _logger.LogInformation("SIAT Bolivia SOAP response received in {ElapsedMs}ms with Status Code {StatusCode}.", 
                stopwatch.ElapsedMilliseconds, response.StatusCode);

            return await response.Content.ReadAsStringAsync();
        }
        catch (OperationCanceledException ex) when (!cts.Token.IsCancellationRequested)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Timeout alcanzado al consumir Web Service SOAP de prueba del SIAT Bolivia en {Url} después de {ElapsedMs}ms (Límite: {TimeoutSeconds}s).", 
                url, stopwatch.ElapsedMilliseconds, timeoutSeconds);
            throw new TimeoutException($"SIAT Bolivia SOAP service timed out after {timeoutSeconds} seconds.", ex);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error crítico consumiendo Web Service SOAP de prueba del SIAT Bolivia en {Url} después de {ElapsedMs}ms.", 
                url, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
