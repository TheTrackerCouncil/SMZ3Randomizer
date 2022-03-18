using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Randomizer.SMZ3.ChatIntegration;

namespace Randomizer.SMZ3.Twitch
{
    public class TwitchAuthenticationService : OAuthChatAuthenticationService
    {
        public override Uri GetOAuthUrl(Uri redirectUri)
        {
            return new Uri("https://id.twitch.tv/oauth2/authorize" +
                "?client_id=i8sfdyflu72jddzpaho80fdt6vc3ax" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri.ToString())}" +
                "&response_type=token" +
                $"&scope={Uri.EscapeDataString("chat:read chat:edit")}");
        }
    }
}
