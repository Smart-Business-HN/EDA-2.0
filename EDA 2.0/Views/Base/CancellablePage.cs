using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EDA_2._0.Views.Base
{
    /// <summary>
    /// Clase base para paginas que necesitan cancelar operaciones async
    /// cuando el usuario navega fuera de la pagina.
    /// </summary>
    public abstract class CancellablePage : Page, IDisposable
    {
        private CancellationTokenSource? _pageCts;
        private bool _disposed;

        /// <summary>
        /// Token de cancelacion que se cancela automaticamente cuando
        /// el usuario navega fuera de la pagina.
        /// </summary>
        protected CancellationToken PageCancellationToken =>
            _pageCts?.Token ?? CancellationToken.None;

        /// <summary>
        /// Verifica si la pagina sigue activa antes de actualizar UI.
        /// </summary>
        protected bool IsPageActive => _pageCts != null && !_pageCts.IsCancellationRequested;

        protected CancellablePage()
        {
            _pageCts = new CancellationTokenSource();
            Loaded += OnPageLoaded;
            Unloaded += OnPageUnloaded;
        }

        private void OnPageLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Crear nuevo CTS si fue cancelado previamente
            if (_pageCts == null || _pageCts.IsCancellationRequested)
            {
                _pageCts?.Dispose();
                _pageCts = new CancellationTokenSource();
            }
        }

        private void OnPageUnloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            CancelAndDisposeToken();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            CancelAndDisposeToken();
        }

        private void CancelAndDisposeToken()
        {
            try
            {
                if (_pageCts != null && !_pageCts.IsCancellationRequested)
                {
                    _pageCts.Cancel();
                }
            }
            catch (ObjectDisposedException)
            {
                // Token ya fue disposed, ignorar
            }
        }

        /// <summary>
        /// Ejecuta una operacion async de forma segura, manejando cancelacion
        /// y excepciones. Usar para event handlers async void.
        /// </summary>
        protected async void SafeExecuteAsync(Func<CancellationToken, Task> operation,
            Action<Exception>? onError = null)
        {
            try
            {
                await operation(PageCancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Operacion cancelada por navegacion, ignorar
                System.Diagnostics.Debug.WriteLine("Operation cancelled due to navigation");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SafeExecuteAsync: {ex.Message}");
                onError?.Invoke(ex);
            }
        }

        /// <summary>
        /// Version de SafeExecuteAsync que retorna un resultado.
        /// </summary>
        protected async void SafeExecuteAsync<T>(Func<CancellationToken, Task<T>> operation,
            Action<T>? onSuccess = null,
            Action<Exception>? onError = null)
        {
            try
            {
                var result = await operation(PageCancellationToken);
                if (IsPageActive)
                {
                    onSuccess?.Invoke(result);
                }
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Operation cancelled due to navigation");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SafeExecuteAsync: {ex.Message}");
                if (IsPageActive)
                {
                    onError?.Invoke(ex);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    CancelAndDisposeToken();
                    _pageCts?.Dispose();
                    _pageCts = null;
                }
                _disposed = true;
            }
        }
    }
}
