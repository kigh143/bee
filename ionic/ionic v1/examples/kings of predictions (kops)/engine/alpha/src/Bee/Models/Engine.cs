using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Bee
{
    public class Engine
    {
        public const string RawNectorName = "nector";
        public const string RawHiveDefiniationCombNodeName = "combs";
        public const string RawHiveDefiniationLocationNodeName = "location";
        public const string RawHiveDefiniationMoodNodeName = "mood";
        public const string RawHiveDefiniationSecurityNodeName = "security";
        public const string RawHiveDefiniationSecurityUserCombNodeName = "userComb";
        public const string RawHiveDefiniationSecurityUserNameCellNodeName = "usernameCell";
        public const string RawHiveDefiniationSecurityPasswordCellNodeName = "passwordCell";
        public const string RawHiveDefiniationSecuritySecretPotionNodeName = "secretPotion";
        public const string RawHiveDefiniationSecurityIsRegistrationOpenNodeName = "isRegistrationOpen";
        public const string RawHiveDefiniationFlowersNodeName = "flowers";
        public const string RawHiveDefiniationSeedsNodeName = "seeds";
        public const string LoginNativeFlowerName = "Login";
        public const string GetUserOfTokenNativeFlowerName = "GetTokenUser";
        public const string RegisterNativeFlowerName = "Register";
        public const string CurrentUserName = "CurrentUser";
        public const string ChangePasswordNativeFlowerName = "ChangePassword";
        public const string ForgotPasswordNativeFlowerName = "ForgotPassword";
        public const string NewPasswordNativeFlowerName = "NewPassword";
        public const string AccivateAccountNativeFlowerName = "ActivateAccount";
        public const string ClearChunksNativeFlowerName = "ClearChunks";
        public const string AddChunksNativeFlowerName = "AddChunks";
        public const string MakeFileNativeFlowerName = "MakeFile";
        

        public const string ActionName = "way";
        public const string GetActionName = "get";
        public const string PostActionName = "post";
        public const string UpdateActionName = "update";
        public const string DeleteActionName = "delete";

        public const string AuthName = "auth";


        public const string PathObjectIndicator = "o";
        public const string PathListIndicator = "l";

        public static JObject Hive;

        
    }
}