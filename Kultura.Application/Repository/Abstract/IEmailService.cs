using Kultura.Application.Model;

namespace Kultura.Application.Repository.Abstract
{
    public interface IEmailService
    {
        void SendEmail(Message message);
    }
}
