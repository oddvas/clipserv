using StreamDeckSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsInput;
using WindowsInput.Native;

namespace Clipserv
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double BUTTON_WIDTH = 72;
        private const double BUTTON_HEIGHT = 72;

        private SynchronizationContext SyncContext = SynchronizationContext.Current;

        private System.Windows.Forms.NotifyIcon _notifyIcon;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            var deck = StreamDeck.OpenDevice();
            deck.SetBrightness(50);
            deck.ShowLogo();
        }

        private void Deck_ConnectionStateChanged(object sender, ConnectionEventArgs e)
        {
            if (e.NewConnectionState)
            {
                if (sender is IStreamDeck deck)
                {
                    InitDeck(deck);
                }
            }
        }

        private void Deck_KeyStateChanged(object sender, StreamDeckSharp.KeyEventArgs e)
        {
            if (e.IsDown)
            {
                SyncContext.Post(async (state) =>
                {
                    if (sender is IStreamDeck deck)
                    {
                        if (e.Key == 0)
                        {
                            var path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                            mediaElement.Source = new Uri(System.IO.Path.Combine(path, "Resources/badum-tss.mp3"));
                            mediaElement.Play();
                            return;
                        }
                        if (e.Key == 1)
                        {
                            var path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                            mediaElement.Source = new Uri(System.IO.Path.Combine(path, "Resources/sad-trombone.mp3"));
                            mediaElement.Play();
                            return;
                        }
                        if (e.Key == 5)
                        {
                            for (ushort i = 0; i <= byte.MaxValue; i++)
                            {
                                deck.SetKeyBitmap(5, KeyBitmap.FromRGBColor((byte)i, (byte)(byte.MaxValue - i), (byte)i));
                                await Task.Delay(10);
                            }
                        }
                        if (e.Key == 4)
                        {
                            var inputSimulator = new InputSimulator();
                            var modifiers = new List<VirtualKeyCode>
                            {
                                VirtualKeyCode.LWIN,
                                VirtualKeyCode.LSHIFT
                            };
                            inputSimulator.Keyboard.ModifiedKeyStroke(modifiers, VirtualKeyCode.VK_S);
                            return;
                        }
                        var imageFromClipboard = Clipboard.GetImage();
                        if (imageFromClipboard != null)
                        {
                            var resizedImage = new TransformedBitmap(imageFromClipboard, new ScaleTransform(BUTTON_WIDTH / imageFromClipboard.PixelWidth, BUTTON_HEIGHT / imageFromClipboard.PixelHeight));
                            var bitmapEncoder = new PngBitmapEncoder();
                            bitmapEncoder.Frames.Add(BitmapFrame.Create(resizedImage));
                            var imageStream = new MemoryStream();
                            bitmapEncoder.Save(imageStream);
                            var keybitmap = KeyBitmap.FromStream(imageStream);
                            deck.SetKeyBitmap(e.Key, keybitmap);
                        }
                    }
                }, null);
                //switch (e.Key)
                //{
                //    case 0:
                //        Console.Write(e.Key);
                //        Console.WriteLine(" - Trondheim Ballklubb");
                //        break;
                //    case 1:
                //        Console.Write(e.Key);
                //        Console.WriteLine(" - Smiley");
                //        break;
                //    case 4:

                //        break;
                //    case 10:
                //        ArrayList selected = new ArrayList();
                //        //foreach (SHDocVw.InternetExplorer window in new SHDocVw.ShellWindows())
                //        //{
                //        //    filename = Path.GetFileNameWithoutExtension(window.FullName).ToLower();
                //        //    if (filename.ToLowerInvariant() == "explorer")
                //        //    {
                //        //        Shell32.FolderItems items = ((Shell32.IShellFolderViewDual2)window.Document).SelectedItems();
                //        //        foreach (Shell32.FolderItem item in items)
                //        //        {
                //        //            selected.Add(item.Path);
                //        //        }
                //        //    }
                //        //}
                //        foreach (var file in selected)
                //        {
                //            Console.WriteLine(file);
                //        }
                //        break;
                //    case 11:


                //        //System.Windows.Clipboard
                //        break;
                //    case 14:
                //        if (sender is IStreamDeck deck)
                //        {
                //            Console.WriteLine("-----------------------");
                //            for (ushort i = 0; i <= byte.MaxValue; i++)
                //            {
                //                deck.SetKeyBitmap(14, KeyBitmap.FromRGBColor((byte)i, (byte)(byte.MaxValue - i), (byte)i));
                //                await Task.Delay(10);
                //                Console.WriteLine(i);
                //            }
                //        }
                //        break;
                //    default:
                //        Console.WriteLine(e.Key);
                //        break;
                //}
            }
        }

        private void InitDeck(IStreamDeck deck)
        {
            deck.SetBrightness(100);
            deck.ClearKeys();
            var bitmap = KeyBitmap.FromFile(@"Resources\drum.png");
            deck.SetKeyBitmap(0, bitmap);
            var bitmapSad = KeyBitmap.FromFile(@"Resources\trumpet.png");
            deck.SetKeyBitmap(1, bitmapSad);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var deck = StreamDeck.OpenDevice();
            InitDeck(deck);

            deck.ConnectionStateChanged += Deck_ConnectionStateChanged;

            deck.KeyStateChanged += Deck_KeyStateChanged;

            Application.Current.Exit += Current_Exit;

            _notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application,
                Visible = true
            };
            ShowInTaskbar = false;
            _notifyIcon.MouseDoubleClick += _notifyIcon_MouseDoubleClick;
        }

        private void _notifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            Hide();
            base.OnStateChanged(e);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _notifyIcon?.Icon?.Dispose();
            _notifyIcon?.Dispose();
        }
    }
}
