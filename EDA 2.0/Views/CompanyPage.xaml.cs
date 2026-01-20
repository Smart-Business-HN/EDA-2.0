using EDA.APPLICATION.Features.CompanyFeature.Commands.UpdateCompanyCommand;
using EDA.APPLICATION.Features.CompanyFeature.Queries;
using EDA.DOMAIN.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace EDA_2._0.Views
{
    public sealed partial class CompanyPage : Page
    {
        private readonly IMediator _mediator;
        private Company? _currentCompany;

        public CompanyPage()
        {
            InitializeComponent();
            _mediator = App.Services.GetRequiredService<IMediator>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCompanyData();
        }

        private async Task LoadCompanyData()
        {
            SetLoading(true);

            try
            {
                var query = new GetCompanyQuery();
                var result = await _mediator.Send(query);

                if (result.Succeeded && result.Data != null)
                {
                    _currentCompany = result.Data;
                    PopulateForm(_currentCompany);
                }
                else
                {
                    ShowError(result.Message ?? "Error al cargar la informacion de la empresa");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void PopulateForm(Company company)
        {
            NameTextBox.Text = company.Name ?? string.Empty;
            OwnerTextBox.Text = company.Owner ?? string.Empty;
            DescriptionTextBox.Text = company.Description ?? string.Empty;
            Address1TextBox.Text = company.Address1 ?? string.Empty;
            Address2TextBox.Text = company.Address2 ?? string.Empty;
            EmailTextBox.Text = company.Email ?? string.Empty;
            PhoneNumber1TextBox.Text = company.PhoneNumber1 ?? string.Empty;
            PhoneNumber2TextBox.Text = company.PhoneNumber2 ?? string.Empty;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveCompanyData();
        }

        private async Task SaveCompanyData()
        {
            SetLoading(true);

            try
            {
                var command = new UpdateCompanyCommand
                {
                    Id = _currentCompany?.Id,
                    Name = NameTextBox.Text?.Trim() ?? string.Empty,
                    Owner = OwnerTextBox.Text?.Trim() ?? string.Empty,
                    Description = string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ? null : DescriptionTextBox.Text.Trim(),
                    Address1 = string.IsNullOrWhiteSpace(Address1TextBox.Text) ? null : Address1TextBox.Text.Trim(),
                    Address2 = string.IsNullOrWhiteSpace(Address2TextBox.Text) ? null : Address2TextBox.Text.Trim(),
                    Email = string.IsNullOrWhiteSpace(EmailTextBox.Text) ? null : EmailTextBox.Text.Trim(),
                    PhoneNumber1 = string.IsNullOrWhiteSpace(PhoneNumber1TextBox.Text) ? null : PhoneNumber1TextBox.Text.Trim(),
                    PhoneNumber2 = string.IsNullOrWhiteSpace(PhoneNumber2TextBox.Text) ? null : PhoneNumber2TextBox.Text.Trim()
                };

                var result = await _mediator.Send(command);

                if (result.Succeeded && result.Data != null)
                {
                    _currentCompany = result.Data;
                    ShowSuccess();
                }
                else
                {
                    ShowError(result.Message ?? "Error al guardar la informacion de la empresa");
                }
            }
            catch (EDA.APPLICATION.Exceptions.ValidationException vex)
            {
                ShowError(string.Join("\n", vex.Errors));
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
            }
            finally
            {
                SetLoading(false);
            }
        }

        private void SetLoading(bool isLoading)
        {
            LoadingRing.IsActive = isLoading;
            SaveButton.IsEnabled = !isLoading;
        }

        private async void ShowError(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new FontIcon
                        {
                            Glyph = "\uEA39",
                            FontSize = 48,
                            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = TextWrapping.Wrap,
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    }
                },
                CloseButtonText = "Aceptar",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private async void ShowSuccess()
        {
            var dialog = new ContentDialog
            {
                Title = "Exito",
                Content = new StackPanel
                {
                    Spacing = 12,
                    Children =
                    {
                        new FontIcon
                        {
                            Glyph = "\uE73E",
                            FontSize = 48,
                            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green),
                            HorizontalAlignment = HorizontalAlignment.Center
                        },
                        new TextBlock
                        {
                            Text = "Informacion guardada exitosamente.",
                            TextWrapping = TextWrapping.Wrap,
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    }
                },
                CloseButtonText = "Aceptar",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
