using System.Runtime.InteropServices;

namespace mauiDemo;

public partial class MainPage : ContentPage
{
    const int RTLD_NOW = 2;

    [DllImport("libdl.so")]
    protected static extern IntPtr dlopen(string filename, int flags);

    [DllImport("libdl.so")]
    protected static extern IntPtr dlsym(IntPtr handle, string symbol);

    [DllImport("libdl.so")]
    protected static extern IntPtr Dlclose(IntPtr handle, string symbol);


    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int NFE_Inicializar(string eArqConfig, string eChaveCrypt);

    public MainPage()
    {
        InitializeComponent();

        string libraryName = "libacbrnfe64.so"; // Substitua pelo nome correto da sua biblioteca
        string functionName = "NFE_Inicializar";

        var lib = NativeLibrary.Load("libdl.so");

        //var acbr = NativeLibrary.Load(libraryName);

        //var acbr = NativeLibrary.Load(libraryName);

        var dlopenFunction = NativeLibrary.GetExport(lib, "dlopen");
        var dlsymFunction = NativeLibrary.GetExport(lib, "dlsym");
        var dlcloseFunction = NativeLibrary.GetExport(lib, "dlclose");

        var dlopenDelegate = Marshal.GetDelegateForFunctionPointer<DlopenDelegate>(dlopenFunction);
        var dlsymDelegate = Marshal.GetDelegateForFunctionPointer<DlsymDelegate>(dlsymFunction);
        var dlcloseDelegate = Marshal.GetDelegateForFunctionPointer<DlcloseDelegate>(dlcloseFunction);

        // Carregar a biblioteca
        IntPtr handle = dlopenDelegate(libraryName, RTLD_NOW);
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


        DisplayAlert("Alert","Botão iniciado","OK");
    }
}

