if exist publish goto :build

md publish

:build
nuget pack -build -sym src\WindowsIntegration\Xpand.ExpressApp.Win.Para.WindowsIntegration.csproj -OutputDirectory publish