using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using DeathCounter;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using VRage.Game;
using VRage.Game.Components;
using VRage.ModAPI;
//using Sandbox.ModAPI.Ingame;

namespace DeathCounter
{
    [MySessionComponentDescriptor(
        MyUpdateOrder.BeforeSimulation | MyUpdateOrder.AfterSimulation | MyUpdateOrder.Simulation )]
    public class Core : MySessionComponentBase
    {
        // Declarations
        private static readonly string version = "v1.0";
        public static Dictionary<IMyPlayer, int> DeathCounter = new Dictionary<IMyPlayer, int>( );

        public static Random random = new Random( );

        private static bool _initialized;

        public const ushort CLIENT_ID = 1699;
        public const ushort SERVER_ID = 1700;

        // Initializers
        private void Initialize( )
        {
            // Chat Line Event
            AddMessageHandler( );

            Logging.Instance.WriteLine( string.Format( "Script Initialized: {0}", version ) );

            MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler( 0, DestroyHandler );
        }

        private static void DestroyHandler( object target, MyDamageInformation info )
        {
            Logging.Instance.WriteLine("Player Died");
            if (!MyAPIGateway.Multiplayer.IsServer)
                return;

            if ( !(target is IMyCharacter) )
                return;

            var character = (IMyEntity)target;
            var player = MyAPIGateway.Players.GetPlayerControllingEntity( character );
            IMyEntity attacker = null;
            if ( !MyAPIGateway.Entities.TryGetEntityById( info.AttackerId, out attacker ) )
            {
                Logging.Instance.WriteLine("Couldn't get attacking entity");
                return;
            }

            var playerName = player.DisplayName;
            string message;
            int index;
            Logging.Instance.WriteLine("evaluating death");
            if(info.Amount==1000)
            {
                index = random.Next( 0, Messages.SuicideStrings.Count( ) - 1 );
                message = Messages.SuicideStrings[index];
                HandleMessage( playerName + message, player );
            }

            if ( info.Type.ToString( ) == "" || playerName == "" )
                return;

            switch ( info.Type.String )
            {
                case "Explosion":
                    index = random.Next( 0, Messages.ExplosionStrings.Count( ) - 1 );
                    message = Messages.ExplosionStrings[index];
                    HandleMessage( playerName + message, player );
                    break;

                case "Rocket":
                    index = random.Next( 0, Messages.RocketStrings.Count( ) - 1 );
                    message = Messages.RocketStrings[index];
                    HandleMessage( playerName + message, player );
                    break;

                case "Bullet":
                    index = random.Next( 0, Messages.BulletStrings.Count( ) - 1 );
                    message = Messages.BulletStrings[index];
                    HandleMessage( playerName + message, player );
                    break;

                case "Drill":
                    index = random.Next( 0, Messages.DrillStrings.Count( ) - 1 );
                    message = Messages.DrillStrings[index];
                    HandleMessage( playerName + message, player );
                    break;

                case "Suicide":
                    index = random.Next( 0, Messages.SuicideStrings.Count( ) - 1 );
                    message = Messages.SuicideStrings[index];
                    HandleMessage( playerName + message, player );
                    break;

                case "Fall":
                    index = random.Next( 0, Messages.FallStrings.Count( ) - 1 );
                    message = Messages.FallStrings[index];
                    HandleMessage( playerName + message, player );
                    break;

                case "Grind":
                    index = random.Next( 0, Messages.GrindStrings.Count( ) - 1 );
                    message = Messages.GrindStrings[index];
                    HandleMessage( playerName + message, player );
                    break;

                case "Weld":
                    index = random.Next( 0, Messages.WeldStrings.Count( ) - 1 );
                    message = Messages.WeldStrings[index];
                    HandleMessage( playerName + message, player );
                    break;

                case "Asphyxia":
                    index = random.Next( 0, Messages.AsphyxiaStrings.Count( ) - 1 );
                    message = Messages.AsphyxiaStrings[index];
                    HandleMessage( playerName + message, player );
                    break;

                case "LowPressure":
                    index = random.Next( 0, Messages.AsphyxiaStrings.Count( ) - 1 );
                    message = Messages.AsphyxiaStrings[index];
                    HandleMessage( playerName + message, player );
                    break;

                case "Environment":
                    if ( attacker.ToString( ).ToLower( ).Contains( "missile" ) )
                    {
                        index = random.Next( 0, Messages.RocketStrings.Count( ) - 1 );
                        message = Messages.RocketStrings[index];
                        HandleMessage( playerName + message, player );
                    }
                    else if ( attacker is IMyFloatingObject )
                    {
                        index = random.Next( 0, Messages.FloatingStrings.Count( ) - 1 );
                        message = Messages.FloatingStrings[index];
                        HandleMessage( playerName + message, player );
                    }
                    else
                    {
                        index = random.Next( 0, Messages.FallStrings.Count( ) - 1 );
                        message = Messages.FallStrings[index];
                        HandleMessage( playerName + message, player );
                    }
                    break;

                default:
                    HandleMessage( string.Format( "{0} died of {1}.", playerName, info.Type.String ), player );
                    break;
            }
        }

        private static void HandleMessage( string message, IMyPlayer player )
        {
            Logging.Instance.WriteLine("HandleMessage");
            if ( !MyAPIGateway.Multiplayer.IsServer )
            {
                return;
            }

            if ( DeathCounter.ContainsKey( player ) )
            {
                DeathCounter[player] = DeathCounter[player] + 1;
            }
            else
            {
                DeathCounter.Add( player, 1 );
            }

            ServerNotificationItem item = new ServerNotificationItem
            {
                notificationMessage = message,
                deadId = player.PlayerID
            };
            Logging.Instance.WriteLine(message);
            string itemMessage = MyAPIGateway.Utilities.SerializeToXML<ServerNotificationItem>( item );
            byte[ ] itemData = Encoding.UTF8.GetBytes( itemMessage );

            MyAPIGateway.Utilities.InvokeOnGameThread( ( ) =>
             {
                 MyAPIGateway.Multiplayer.SendMessageToOthers( CLIENT_ID, itemData );
             } );
        }

        // Utility
        public void HandleMessageEntered( string messageText, ref bool sendToOthers )
        {
            try
            {
                if ( messageText == "/death counter" )
                {
                    var data = Encoding.UTF8.GetBytes( MyAPIGateway.Session.LocalHumanPlayer.SteamUserId.ToString( ) );
                    sendToOthers = false;

                    MyAPIGateway.Utilities.InvokeOnGameThread( ( ) =>
                     {
                         MyAPIGateway.Multiplayer.SendMessageToServer( SERVER_ID, data );
                     } );
                }
            }
            catch ( Exception ex )
            {
                Logging.Instance.WriteLine( "HandleMessageEntered()" + ex );
            }
        }

        public void HandleServerData( byte[ ] data )
        {
            Logging.Instance.WriteLine( string.Format( "Received Server Data: {0} bytes", data.Length ) );

            if ( MyAPIGateway.Multiplayer.IsServer && MyAPIGateway.Session.LocalHumanPlayer == null )
                return;

            string message = "";
            try
            {
                message = Encoding.UTF8.GetString( data );
            }
            catch
            {
                return;
            }

           // ServerNotificationItem item = new ServerNotificationItem( );

            ServerNotificationItem item = MyAPIGateway.Utilities.SerializeFromXML<ServerNotificationItem>( message );

            if ( item.dialogMessage != "" )
                Dialog( item.dialogMessage );

            if ( item.notificationMessage != "" )
                Notification( item.notificationMessage, item.deadId );


        }

        public void HandlePlayerData( byte[ ] data )
        {
            //the only reason the client sends data to server is to request a count dialog
            string text = "";
            try
            {
                text = Encoding.UTF8.GetString( data );
            }
            catch { return; }

            ulong steamId;
            if ( !ulong.TryParse( text, out steamId ) )
                return;

            ServerNotificationItem item = new ServerNotificationItem( );

            if ( DeathCounter.Any( ) )
            {
                foreach ( IMyPlayer player in Core.DeathCounter.Keys )
                {
                    item.dialogMessage += string.Format( "{0} deaths: {1}|", player.DisplayName, Core.DeathCounter[player] );
                }
            }
            else
            {
                item.dialogMessage = "No one has died yet.";
            }

            item.notificationMessage = "";

            string itemMessage = MyAPIGateway.Utilities.SerializeToXML( item );
            byte[ ] itemData = Encoding.UTF8.GetBytes( itemMessage );

            MyAPIGateway.Utilities.InvokeOnGameThread( ( ) =>
             {
                 MyAPIGateway.Multiplayer.SendMessageTo( CLIENT_ID, itemData, steamId );
             } );
        }

        public void Dialog( string message )
        {
            MyAPIGateway.Utilities.ShowMissionScreen( "Death Counter", "", "", message.Replace( "|", "\n\r" ), null, "close" );
        }

        public void Notification( string message, long playerId )
        {

            var relation =
                MyAPIGateway.Session.LocalHumanPlayer.GetRelationTo( playerId );

            if ( MyAPIGateway.Session.LocalHumanPlayer.PlayerID == playerId )
                MyAPIGateway.Utilities.ShowNotification( message, 5000, MyFontEnum.Blue );
            else if ( relation == MyRelationsBetweenPlayerAndBlock.Enemies )
                MyAPIGateway.Utilities.ShowNotification( message, 5000, MyFontEnum.Red );
            else if ( relation == MyRelationsBetweenPlayerAndBlock.Neutral )
                MyAPIGateway.Utilities.ShowNotification( message, 5000, MyFontEnum.Green );
            else
                MyAPIGateway.Utilities.ShowNotification( message, 5000, MyFontEnum.DarkBlue );
        }

        public void AddMessageHandler( )
        {
            MyAPIGateway.Utilities.MessageEntered += HandleMessageEntered;
            MyAPIGateway.Multiplayer.RegisterMessageHandler( CLIENT_ID, HandleServerData );
            MyAPIGateway.Multiplayer.RegisterMessageHandler( SERVER_ID, HandlePlayerData );
        }

        public void RemoveMessageHandler( )
        {
            MyAPIGateway.Utilities.MessageEntered -= HandleMessageEntered;
            MyAPIGateway.Multiplayer.UnregisterMessageHandler( CLIENT_ID, HandleServerData );
            MyAPIGateway.Multiplayer.UnregisterMessageHandler( SERVER_ID, HandlePlayerData );
        }

        // Overrides
        public override void UpdateBeforeSimulation( )
        {
            try
            {
                if ( MyAPIGateway.Session == null )
                    return;

                // Run the init
                if ( !_initialized )
                {
                    _initialized = true;
                    Initialize( );
                }
            }
            catch ( Exception ex )
            {
                Logging.Instance.WriteLine( string.Format( "UpdateBeforeSimulation(): {0}", ex ) );
            }
        }

        public override void UpdateAfterSimulation( )
        {
        }

        protected override void UnloadData( )
        {
            try
            {
                RemoveMessageHandler( );

                if ( Logging.Instance != null )
                    Logging.Instance.Close( );
            }
            catch
            {
            }

            base.UnloadData( );
        }
    }

    public class ServerNotificationItem
    {
        public string dialogMessage = "";

        public string notificationMessage = "";

        public long deadId;

    }
}