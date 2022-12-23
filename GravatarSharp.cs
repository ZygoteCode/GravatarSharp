using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Drawing;
using System.IO;
using System.ComponentModel.DataAnnotations;
using LegitHttpClient;

public class GravatarSharp
{
    public static byte[] GetGravatarImageBytes(string email, int size = 80)
    {
        if (!IsEmailValid(email))
        {
            throw new Exception("The specified e-mail address is not in a valid format.");
        }

        try
        {
            HttpClient client = new HttpClient();
            client.ByteRate = 8192;
            client.ConnectTo("s.gravatar.com", true, 443);

            HttpRequest request = new HttpRequest();
            request.URI = $"/avatar/{GetHash(email)}?s={size}";
            request.Method = HttpMethod.GET;
            request.Version = HttpVersion.HTTP_11;
            request.Headers.Add(new HttpHeader() { Name = "Host", Value = "s.gravatar.com" });

            HttpResponse response = client.Send(request);
            byte[] body = response.Body;

            while (!(body[0] == 0x89 && body[1] == 0x50 && body[2] == 0x4E && body[3] == 0x47))
            {
                body = body.Skip(1).ToArray();
            }

            return body;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get the Gravatar image of {email}\r\n{ex.Message}\r\n{ex.StackTrace}\r\n{ex.Source}");
        }
    }

    public static Image GetGravatarImage(string email, int size = 80)
    {
        using (MemoryStream memoryStream = new MemoryStream(GetGravatarImageBytes(email, size)))
        {
            return Image.FromStream(memoryStream);
        }
    }

    private static string GetHash(string inputString)
    {
        StringBuilder sb = new StringBuilder();

        foreach (byte b in MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(inputString)))
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }

    private static bool IsEmailValid(string email)
    {
        if (email.Length > 320)
        {
            return false;
        }

        if (!(new EmailAddressAttribute().IsValid(email)))
        {
            return false;
        }

        var trimmedEmail = email.Trim();

        if (trimmedEmail.EndsWith(".") || trimmedEmail.Contains("+"))
        {
            return false;
        }

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);

            if (addr.Address != trimmedEmail)
            {
                return false;
            }
        }
        catch
        {
            return false;
        }

        return true;
    }
}