using StreamDeckSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shell32;

namespace ClipservWindows
{
    public class Program
    {
        static ManualResetEvent _quitEvent = new ManualResetEvent(false);

        public static void Main(string[] args)
        {
            Console.CancelKeyPress += (sender, eArgs) => {
                _quitEvent.Set();
                eArgs.Cancel = true;
            };

            var deck = StreamDeck.OpenDevice();
            InitDeck(deck);

            deck.ConnectionStateChanged += Deck_ConnectionStateChanged;

            deck.KeyStateChanged += Deck_KeyStateChanged;

            _quitEvent.WaitOne();
            deck.SetBrightness(50);
            deck.ShowLogo();
        }

        private static void Deck_ConnectionStateChanged(object sender, ConnectionEventArgs e)
        {
            Console.Write("New connection state: ");
            Console.WriteLine(e.NewConnectionState);
            if (e.NewConnectionState)
            {
                if (sender is IStreamDeck deck)
                {
                    InitDeck(deck);
                }
                
            }
        }

        private static async void Deck_KeyStateChanged(object sender, KeyEventArgs e)
        {
            if (e.IsDown)
            {
                switch (e.Key)
                {
                    case 0:
                        Console.Write(e.Key);
                        Console.WriteLine(" - Trondheim Ballklubb");
                        break;
                    case 1:
                        Console.Write(e.Key);
                        Console.WriteLine(" - Smiley");
                        break;
                    case 4:
                        _quitEvent.Set();
                        break;
                    case 10:
                        string filename;
                        ArrayList selected = new ArrayList();
                        foreach (SHDocVw.InternetExplorer window in new SHDocVw.ShellWindows())
                        {
                            filename = Path.GetFileNameWithoutExtension(window.FullName).ToLower();
                            if (filename.ToLowerInvariant() == "explorer")
                            {
                                Shell32.FolderItems items = ((Shell32.IShellFolderViewDual2)window.Document).SelectedItems();
                                foreach (Shell32.FolderItem item in items)
                                {
                                    selected.Add(item.Path);
                                }
                            }
                        }
                        foreach (var file in selected)
                        {
                            Console.WriteLine(file);
                        }
                        break;
                    case 11:
                        //System.Windows.Clipboard
                        break;
                    case 14:
                        if (sender is IStreamDeck deck)
                        {
                            Console.WriteLine("-----------------------");
                            for (ushort i = 0; i <= byte.MaxValue; i++)
                            {
                                deck.SetKeyBitmap(14, KeyBitmap.FromRGBColor((byte)i, (byte)(byte.MaxValue - i), (byte)i));
                                await Task.Delay(10);
                                Console.WriteLine(i);
                            }
                        }
                        break;
                    default:
                        Console.WriteLine(e.Key);
                        break;
                }
            }
        }

        private static void InitDeck(IStreamDeck deck)
        {
            deck.SetBrightness(100);
            deck.ClearKeys();
            var bitmap = KeyBitmap.FromFile(@"C:\Users\oddva\Downloads\streamdeck_key.png");
            deck.SetKeyBitmap(1, bitmap);
            var bitmapTBK = KeyBitmap.FromFile(@"C:\Users\oddva\Downloads\tbk.png");
            deck.SetKeyBitmap(0, bitmapTBK);
            var bitmapPanic = KeyBitmap.FromFile(@"C:\Users\oddva\Downloads\power_icon.png");
            deck.SetKeyBitmap(4, bitmapPanic);
            var bitmapCopy1 = KeyBitmap.FromRGBColor(55, 200, 55);
            deck.SetKeyBitmap(10, bitmapCopy1);
            var bitmapCopy2 = KeyBitmap.FromRGBColor(255, 200, 55);
            deck.SetKeyBitmap(11, bitmapCopy2);
        }
    }
}
