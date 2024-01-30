using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace mauiDemo;

public partial class MainPage : ContentPage
{
    const int RTLD_NOW = 2;

    ILogger<MainPage> _logger;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    delegate IntPtr DlopenDelegate(string fileName, int flags);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    delegate IntPtr DlsymDelegate(IntPtr handle, string symbol);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    delegate int DlcloseDelegate(IntPtr handle);

    private const string DllName = "libacbrnfe64.so";


    [DllImport(DllName, EntryPoint = "NFE_Inicializar")]
    protected static extern IntPtr NFEInicializar(string filename, int flags);



    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int NFE_Inicializar(string eArqConfig, string eChaveCr);

    public MainPage(ILogger<MainPage> logger)
    {
        InitializeComponent();

        _logger = logger;

        string libraryName = "libacbrnfe64.so"; // Substitua pelo nome correto da sua biblioteca
        string functionName = "NFE_Inicializar";


        var lib = NativeLibrary.Load("libdl.so");

        var dlopenFunction = NativeLibrary.GetExport(lib, "dlopen");
        var dlsymFunction = NativeLibrary.GetExport(lib, "dlsym");
        var dlcloseFunction = NativeLibrary.GetExport(lib, "dlclose");

        var dlopenDelegate = Marshal.GetDelegateForFunctionPointer<DlopenDelegate>(dlopenFunction);
        var dlsymDelegate = Marshal.GetDelegateForFunctionPointer<DlsymDelegate>(dlsymFunction);
        var dlcloseDelegate = Marshal.GetDelegateForFunctionPointer<DlcloseDelegate>(dlcloseFunction);



        var resultdll = NFEInicializar("", 2);
        // Carregar a biblioteca
        IntPtr handle = dlopenDelegate(libraryName, 1);
        if (handle == IntPtr.Zero)
        {
            Console.WriteLine($"Não foi possível carregar a biblioteca: {libraryName}");
            return;
        }
       

        // Obter um ponteiro para a função
        IntPtr functionPtr = dlsymDelegate(handle, functionName);
        if (functionPtr == IntPtr.Zero)
        {
            Console.WriteLine($"Não foi possível obter um ponteiro para a função: {functionName}");
            dlcloseDelegate(handle);
            return;
        }

        // Converter o ponteiro para um delegate
        var functionDelegate = Marshal.GetDelegateForFunctionPointer<NFE_Inicializar>(functionPtr);

        // Chamar a função
        int result = (int)functionDelegate.DynamicInvoke();
        Console.WriteLine($"Resultado da chamada da função: {result}");

        // Fechar a biblioteca
        dlcloseDelegate(handle);
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {

        _logger.LogInformation($"Clicked");

        DisplayAlert("Alert", "Botão iniciado", "OK");
    }
}

