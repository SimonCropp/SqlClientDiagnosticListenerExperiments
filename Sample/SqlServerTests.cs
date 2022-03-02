using System.Diagnostics;

public class SqlServerTests
{
    static SqlInstance sqlInstance = new(
            "SqlServerTests",
            connection =>
            {
                var sqlConnection = (SqlConnection) connection;
                var server = new Server(new ServerConnection(sqlConnection));
                server.ConnectionContext.ExecuteNonQuery(@"
CREATE TABLE
MyTable(Value int);
GO
");
                return Task.CompletedTask;
            });

    [Fact]
    public async Task Run()
    {
        using var database = await sqlInstance.Build();
        DiagnosticListener.AllListeners.Subscribe(new Subscriber());
        using var command = database.Connection.CreateCommand();
        command.CommandText = "select Value from MyTable";
        command.ExecuteScalar();
        Assert.True(Subscriber.SqlOnNextHit);
    }
}

public class Subscriber : IObserver<DiagnosticListener>
{
    public void OnCompleted() { }

    public void OnError(Exception error) { }

    public void OnNext(DiagnosticListener listener)
    {
        if (listener.Name == "SqlClientDiagnosticListener")
        {
            SqlOnNextHit = true;
        }
    }

    public static bool SqlOnNextHit;
}