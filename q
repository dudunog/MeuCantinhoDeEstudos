[1mdiff --git a/MeuCantinhoDeEstudos3.csproj b/MeuCantinhoDeEstudos3.csproj[m
[1mindex 7ec3de6..2beda33 100644[m
[1m--- a/MeuCantinhoDeEstudos3.csproj[m
[1m+++ b/MeuCantinhoDeEstudos3.csproj[m
[36m@@ -52,6 +52,9 @@[m
     <Reference Include="EntityFramework.SqlServer, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089, processorArchitecture=MSIL">[m
       <HintPath>packages\EntityFramework.6.4.4\lib\net45\EntityFramework.SqlServer.dll</HintPath>[m
     </Reference>[m
[32m+[m[32m    <Reference Include="FastMember, Version=1.5.0.0, Culture=neutral, processorArchitecture=MSIL">[m
[32m+[m[32m      <HintPath>packages\FastMember.1.5.0\lib\net461\FastMember.dll</HintPath>[m
[32m+[m[32m    </Reference>[m
     <Reference Include="Microsoft.CSharp" />[m
     <Reference Include="Microsoft.Extensions.DependencyInjection.Abstractions, Version=2.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60, processorArchitecture=MSIL">[m
       <HintPath>packages\Microsoft.Extensions.DependencyInjection.Abstractions.2.0.0\lib\netstandard2.0\Microsoft.Extensions.DependencyInjection.Abstractions.dll</HintPath>[m
[1mdiff --git a/Web.config b/Web.config[m
[1mindex e37489a..0ee71f5 100644[m
[1m--- a/Web.config[m
[1m+++ b/Web.config[m
[36m@@ -10,9 +10,9 @@[m
   </configSections>[m
   <connectionStrings>[m
     <!-- SQLEXPRESS -->[m
[31m-    <add name="DefaultConnection" connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=MeuCantinhoDeEstudos;Integrated Security=True;MultipleActiveResultSets=True" providerName="System.Data.SqlClient" />[m
[32m+[m[32m    <!--<add name="DefaultConnection" connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=MeuCantinhoDeEstudos;Integrated Security=True;MultipleActiveResultSets=True" providerName="System.Data.SqlClient" />-->[m
     <!--SQLSERVER-->[m
[31m-    <!--<add name="DefaultConnection" connectionString="Data Source=(local);Initial Catalog=MeuCantinhoDeEstudos;Integrated Security=True;MultipleActiveResultSets=True" providerName="System.Data.SqlClient" />-->[m
[32m+[m[32m    <add name="DefaultConnection" connectionString="Data Source=(local);Initial Catalog=MeuCantinhoDeEstudos;Integrated Security=True;MultipleActiveResultSets=True" providerName="System.Data.SqlClient" />[m
   </connectionStrings>[m
   <appSettings>[m
     <add key="webpages:Version" value="3.0.0.0" />[m
[1mdiff --git a/packages.config b/packages.config[m
[1mindex bb59eea..c73aaae 100644[m
[1m--- a/packages.config[m
[1m+++ b/packages.config[m
[36m@@ -3,6 +3,7 @@[m
   <package id="Antlr" version="3.5.0.2" targetFramework="net48" />[m
   <package id="bootstrap" version="3.4.1" targetFramework="net48" />[m
   <package id="EntityFramework" version="6.4.4" targetFramework="net48" />[m
[32m+[m[32m  <package id="FastMember" version="1.5.0" targetFramework="net48" />[m
   <package id="jQuery" version="3.4.1" targetFramework="net48" />[m
   <package id="jQuery.Validation" version="1.17.0" targetFramework="net48" />[m
   <package id="Microsoft.AspNet.Identity.Core" version="2.2.3" targetFramework="net48" />[m
