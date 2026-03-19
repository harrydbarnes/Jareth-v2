using System.Threading.Tasks;
using Jareth.Core.Models;

namespace Jareth.Core.Services;

public interface ISettingsService
{
    AppSettings Settings { get; }
    Task LoadAsync();
    Task SaveAsync();
}
