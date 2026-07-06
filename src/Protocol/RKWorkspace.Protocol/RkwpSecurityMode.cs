namespace RKWorkspace.Protocol;

public enum RkwpSecurityMode
{
    DevelopmentInsecure = 0,
    Authenticated = 1,
    DevelopmentAuthenticated = Authenticated,
    Encrypted = 2,
    TestSecure = Encrypted,
    EncryptedAndAuthenticated = 3,
    ProductionSecure = EncryptedAndAuthenticated,
    ProductionRequired = 4
}
