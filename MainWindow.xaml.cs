﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace RAMAS_PRELIM_ENIGMA
{
    /// test
    ///newest push of 3:40am
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
        {
            // Constants for rotor wiring and reflector
            string _control = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // Standard alphabet for reference
            string _ring1 = "DMTWSILRUYQNKFEJCAZBPGXOHV"; // Rotor 1 wiring (Hours)
            string _ring2 = "HQZGPJTMOBLNCIFDYAWVEUSRKX"; // Rotor 2 wiring (Minutes)
            string _ring3 = "UQNTLSZFMREHDPXKIBVYGJCWOA"; // Rotor 3 wiring (Seconds)
            string _reflector = "YRUHQSLDPXNGOKMIEBFZCWVJAT"; // Reflector wiring

            // Rotor offset tracking
            int[] _keyOffset = { 0, 0, 0 }; // Current rotor offsets (H, M, S)
            int[] _initOffset = { 0, 0, 0 }; // Initial rotor offsets (H, M, S)

            // Rotor state flag
            bool _rotor = false;

            // Plugboard setup
            Dictionary<char, char> _plugboard = new Dictionary<char, char>(); // Plugboard dictionary
            private bool _plugboardSet = false; // Flag to indicate if plugboard is set

            // Constructor
            public MainWindow()
            {
                InitializeComponent();

                SetDefaults(); // Initialize default values

                _rotor = false; // Initially rotor is off
                btnRotor.Content = "Rotor On"; // Set button text
                btnRotor.IsEnabled = false; // Disable rotor button until plugboard is set

                DisplayRing(_lblreflector, _reflector);
            }

            // Display rotor wiring in UI labels
            private void DisplayRing(Label displayLabel, string ring)
            {
                string temp = "";
                foreach (char r in ring)
                    temp += r + "\t"; // Add tab for spacing
                displayLabel.Content = temp;
            }

            // Find the index of a character in a string
            private int IndexSearch(string ring, char letter)
            {
                int index = 0;
                for (int x = 0; x < ring.Length; x++)
                {
                    if (ring[x] == letter)
                    {
                        index = x;
                        break;
                    }
                }
                return index;
            }

            // Handle keyboard input
            private void Window_KeyDown(object sender, KeyEventArgs e)
            {
            if (!_rotor) return; // If rotors are not set, do nothing (no warning spam)

              string key = e.Key.ToString();

            if (key.Length == 1 && key[0] >= 'A' && key[0] <= 'Z' && lblInput.Content.ToString().Length < 128)
            {
                lblInput.Content += key;
                lblEncrpyt.Content += Encrypt(key[0]).ToString();
                lblEncrpytMirror.Content += Mirror(key[0]).ToString();

                Label letterLabel = FindName(key) as Label;
                if (letterLabel != null)
                {
                    letterLabel.Background = Brushes.LightCyan;
                    _ = Task.Delay(300).ContinueWith(_ =>
                    {
                        Dispatcher.Invoke(() => letterLabel.Background = Brushes.Transparent);
                    });
                }

                Rotate(true);
                DisplayRing(lblRing1, _ring1);
                DisplayRing(lblRing2, _ring2);
                DisplayRing(lblRing3, _ring3);
            }
            else if (e.Key == Key.Space)
            {
                lblInput.Content += " ";
                lblEncrpyt.Content += " ";
                lblEncrpytMirror.Content += " ";
            }
            else if (e.Key == Key.Back && lblInput.Content.ToString().Length > 0)
            {
                lblInput.Content = RemoveLastLetter(lblInput.Content.ToString());
                lblEncrpyt.Content = RemoveLastLetter(lblEncrpyt.Content.ToString());
                lblEncrpytMirror.Content = RemoveLastLetter(lblEncrpytMirror.Content.ToString());
            }
        }



        private void DisableEncryption()
        {
            lblInput.Content = "Set rotors first!";
            lblEncrpyt.Content = "";
            lblEncrpytMirror.Content = "";
            lblInput.IsEnabled = false;
            lblEncrpyt.IsEnabled = false;
            lblEncrpytMirror.IsEnabled = false;
        }

        private void EnableEncryption()
        {
            lblInput.Content = "";
            lblEncrpyt.Content = "";
            lblEncrpytMirror.Content = "";
            lblInput.IsEnabled = true;
            lblEncrpyt.IsEnabled = true;
            lblEncrpytMirror.IsEnabled = true;
        }

        //new
        // Encrypt a character
        private char Encrypt(char letter)
            {
                char newChar = letter;

                // Plugboard pass (before rotors)
                if (_plugboard.ContainsKey(newChar))
                    newChar = _plugboard[newChar];
                else if (_plugboard.ContainsValue(newChar))
                    newChar = _plugboard.FirstOrDefault(x => x.Value == newChar).Key;

                // Rotor pass forward
                newChar = _ring1[IndexSearch(_control, newChar)];
                newChar = _ring2[IndexSearch(_control, newChar)];
                newChar = _ring3[IndexSearch(_control, newChar)];

                // Reflector pass
                newChar = _reflector[IndexSearch(_control, newChar)];

                // Rotor pass backward
                newChar = _ring3[IndexSearch(_control, newChar)];
                newChar = _ring2[IndexSearch(_control, newChar)];
                newChar = _ring1[IndexSearch(_control, newChar)];

                // Plugboard pass (after rotors)
                if (_plugboard.ContainsKey(newChar))
                    newChar = _plugboard[newChar];
                else if (_plugboard.ContainsValue(newChar))
                    newChar = _plugboard.FirstOrDefault(x => x.Value == newChar).Key;

                return newChar;
            }

            // Mirror a character (encrypt and pass back through rotors)
            private char Mirror(char letter)
            {
                char newChar = Encrypt(letter);

                newChar = _ring3[IndexSearch(_control, newChar)];
                newChar = _ring2[IndexSearch(_control, newChar)];
                newChar = _ring1[IndexSearch(_control, newChar)];

                return newChar;
            }

            // Set default values
            private void SetDefaults()
            {
                _control = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                _ring1 = "DMTWSILRUYQNKFEJCAZBPGXOHV";
                _ring2 = "HQZGPJTMOBLNCIFDYAWVEUSRKX";
                _ring3 = "UQNTLSZFMREHDPXKIBVYGJCWOA";
                _keyOffset = new int[] { 0, 0, 0 };

                lblInput.Content = "";
                lblEncrpyt.Content = "";
                lblEncrpytMirror.Content = "";

                DisplayRing(lblControlRing, _control);
                DisplayRing(lblRing1, _ring1);
                DisplayRing(lblRing2, _ring2);
                DisplayRing(lblRing3, _ring3);
            }

            // Rotate rotors
            private void Rotate(bool forward)
            {
                if (forward)
                {
                    _keyOffset[2]++;
                    _ring1 = MoveValues(forward, _ring1);

                    if (_keyOffset[2] / _control.Length >= 1)
                    {
                        _keyOffset[2] = 0;
                        _keyOffset[1]++;
                        _ring2 = MoveValues(forward, _ring2);
                        if (_keyOffset[1] / _control.Length >= 1)
                        {
                            _keyOffset[1] = 0;
                            _keyOffset[0]++;
                            _ring3 = MoveValues(forward, _ring3);
                        }
                    }
                }
                else
                {
                    if (_keyOffset[2] > 0 || _keyOffset[1] > 0)
                    {
                        _keyOffset[2]--;
                        _ring1 = MoveValues(forward, _ring1);
                        if (_keyOffset[2] < 0)
                        {
                            _keyOffset[2] = 25;
                            _keyOffset[1]--;
                            _ring2 = MoveValues(forward, _ring2);
                            if (_keyOffset[1] < 0)
                            {
                                _keyOffset[1] = 25;
                                _keyOffset[0]--;
                                _ring3 = MoveValues(forward, _ring3);
                                if (_keyOffset[0] < 0)
                                    _keyOffset[0] = 25;
                            }
                        }
                    }
                }

                DisplayOffset(); // Update offset display
            }

            // Move rotor values
            private string MoveValues(bool forward, string ring)
            {
                char movingValue = ' ';
                string newRing = "";

                if (forward)
                {
                    movingValue = ring[0];
                    for (int x = 1; x < ring.Length; x++)
                        newRing += ring[x];
                    newRing += movingValue;
                }
                else
                {
                    movingValue = ring[25];
                    for (int x = 0; x < ring.Length - 1; x++)
                        newRing += ring[x];
                    newRing = movingValue + newRing;
                }

                return newRing;
            }

            // Handle rotor button click
            private void btnRotor_Click(object sender, RoutedEventArgs e)
            {
            SetDefaults();

            if (!int.TryParse(txtBRing1Init.Text, out _initOffset[0]) ||
                !int.TryParse(txtBRing2Init.Text, out _initOffset[1]) ||
                !int.TryParse(txtBRing3Init.Text, out _initOffset[2]))
            {
                MessageBox.Show("Please enter valid integer values (0-25) for rotor settings.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                DisableEncryption(); // Disable encryption-related UI
                return;
            }

            if (_initOffset[0] < 0 || _initOffset[0] > 25 ||
                _initOffset[1] < 0 || _initOffset[1] > 25 ||
                _initOffset[2] < 0 || _initOffset[2] > 25)
            {
                MessageBox.Show("Rotor values must be between 0 and 25.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                DisableEncryption(); // Disable encryption-related UI
                return;
            }

            // If all values are valid, enable encryption and lock settings
            txtBRing1Init.IsEnabled = false;
            txtBRing2Init.IsEnabled = false;
            txtBRing3Init.IsEnabled = false;

            _rotor = true;
            btnRotor.Content = "Settings Lock";
            EnableEncryption(); // Enable encryption-related UI

            // Initialize rotor positions
            _ring1 = InitializeRotors(_initOffset[0], _ring1);
            _ring2 = InitializeRotors(_initOffset[1], _ring2);
            _ring3 = InitializeRotors(_initOffset[2], _ring3);

            DisplayRing(lblRing1, _ring1);
            DisplayRing(lblRing2, _ring2);
            DisplayRing(lblRing3, _ring3);
            DisplayOffset();
        }

            // Initialize rotors with initial offset
            private string InitializeRotors(int initial, string ring)
            {
                string newRing = ring;
                for (int x = 0; x < initial; x++)
                    newRing = MoveValues(true, newRing);
                return newRing;
            }

            // Remove last letter from a string
            private string RemoveLastLetter(string word)
            {
                string newWord = "";
                for (int x = 0; x < word.Length - 1; x++)
                    newWord += word[x];
                return newWord;
            }

            // Handle text box focus
            private void txtBRing1Init_GotFocus(object sender, RoutedEventArgs e)
            {
                txtBRing1Init.Text = "";
            }

            private void txtBRing2Init_GotFocus(object sender, RoutedEventArgs e)
            {
                txtBRing2Init.Text = "";
            }

            private void txtBRing3Init_GotFocus(object sender, RoutedEventArgs e)
            {
                txtBRing3Init.Text = "";
            }

            // Display rotor offsets
            private void DisplayOffset()
            {
                txtBRing1Init.Text = _keyOffset[0] + "";
                txtBRing2Init.Text = _keyOffset[1] + "";
                txtBRing3Init.Text = _keyOffset[2] + "";
            }

            // Setup plugboard
            private void SetupPlugboard(string plugboardSettings)
            {
            _plugboard.Clear();
            string[] pairs = plugboardSettings.ToUpper().Split(' ');
            HashSet<char> usedLetters = new HashSet<char>();

            foreach (string pair in pairs)
            {
                if (pair.Length != 2 || pair[0] == pair[1] || usedLetters.Contains(pair[0]) || usedLetters.Contains(pair[1]))
                {
                    MessageBox.Show("Invalid plugboard settings! Use non-repeating letter pairs (e.g., 'AB CD EF').\n\nPlugboard is set to normal contorl !", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _plugboard[pair[0]] = pair[1];
                _plugboard[pair[1]] = pair[0];
                usedLetters.Add(pair[0]);
                usedLetters.Add(pair[1]);
            }

               _plugboardSet = true;
                txtPlugboard.IsEnabled = false;
               btnRotor.IsEnabled = true;
                MessageBox.Show("Plugboard successfully set!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

            // Handle plugboard button click
            private void btnSetPlugboard_Click(object sender, RoutedEventArgs e)
            {
                if (_plugboardSet)
                {
                    MessageBox.Show("Plugboard is already set.");
                    return;
                }

                SetupPlugboard(txtPlugboard.Text);
                _plugboardSet = true;
                btnRotor.IsEnabled = true; // Enable the rotor button.

                // Explicitly check the flag and perform an action
                if (_plugboardSet)
                {
                    txtPlugboard.IsEnabled = false;
                }
            }

            // Handle plugboard text change
            private void txtPlugboard_TextChanged(object sender, TextChangedEventArgs e)
            {
                lblInput.Content = "";
                lblEncrpyt.Content = "";
                lblEncrpytMirror.Content = "";
            }
        }
}
