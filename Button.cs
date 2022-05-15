using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.Diagnostics;

namespace FishGame
{
    class Button
    {
        // Representerar en knapp. Då MonoGame inte har en egen knapphanterare så har jag skapat en egen.

        public Tuple<Texture2D, Texture2D> AssociatedAssets { get; set; } // Bilder som associeras med knappen när man har muspekaren över den respektive när man inte har det
        public Vector2 Position { get; set; } // Knappens position på skärmen

        public GameState ActiveOnScreen { get; set; } // Vilken skärm (titelskärmen, spelskärmen, etc.) som knappen finns tillgänglig på
        public Texture2D ActiveAsset { get; set; } // Den aktiva texturen som knappen visar
        public int Scale { get; set; } // Vilken skala knappen ritas ut i

        private SoundEffectInstance _buttonClickSound { get; set; } // Ljud när knapp klickas på
        private SoundEffectInstance _buttonHoverSound { get; set; } // Ljud när knapp hålls över

        private enum ButtonStates // Knappens möjliga statusar
        {
            Idle,
            Hovered,
            Clicked
        }

        private ButtonStates _previousButtonState { get; set; }

        public Button(
            string baseAssociatedAssetName,
            ContentManager contentLoader,
            Vector2 position,
            GameState activeOnScreen,
            int scale = 1
        )
        {
            AssociatedAssets = new Tuple<Texture2D, Texture2D>(
                contentLoader.Load<Texture2D>(baseAssociatedAssetName),
                contentLoader.Load<Texture2D>(baseAssociatedAssetName + "-hovered")
            ); // Hämta bilder till knappen
            ActiveAsset = AssociatedAssets.Item1;
            Scale = scale;
            Position = position;
            ActiveOnScreen = activeOnScreen;
            // Ljud när knapp klickas på respektive hålls över
            _buttonClickSound = contentLoader.Load<SoundEffect>("button-click").CreateInstance();
            _buttonHoverSound = contentLoader.Load<SoundEffect>("button-hovered").CreateInstance();
        }

        /// <summary>
        /// Hämtar knappens aktuella status
        /// </summary>
        /// <param name="mouseState">Objekt för att hämta musens koordinater</param>
        /// <param name="previousMouseState">Objekt som representerar musens tidigare status. Används för att motverka att knappen markeras som klickad när muspekaren tidigare hållts nere utan att släppas</param>
        /// <returns></returns>
        private ButtonStates getButtonState(MouseState mouseState, MouseState previousMouseState)
        {
            if (
                mouseState.X >= Position.X
                && mouseState.X <= Position.X + ActiveAsset.Width * Scale
                && mouseState.Y >= Position.Y
                && mouseState.Y <= Position.Y + ActiveAsset.Height * Scale
            ) // Muspekaren hålls vid knappen
            {
                if (
                    mouseState.LeftButton == ButtonState.Pressed
                    && previousMouseState.LeftButton != ButtonState.Pressed
                )
                { // Om knappen har tryckts på, spela klickljud och returnera status
                    if (_buttonClickSound.State != SoundState.Playing)
                    {
                        _buttonClickSound.Play();
                    }
                    _previousButtonState = ButtonStates.Clicked;
                }
                else
                {
                    if (
                        _buttonHoverSound.State != SoundState.Playing
                        && _previousButtonState != ButtonStates.Hovered
                    )
                    {
                        _buttonHoverSound.Play();
                    }
                    _previousButtonState = ButtonStates.Hovered;
                }
            }
            else
            {
                _previousButtonState = ButtonStates.Idle;
            }
            return _previousButtonState;
        }

        /// <summary>
        /// Hämtar vilken bild som ska visas för knappen. Om någon håller muspekaren över knappen så vill vi ju att den ska visas som "markerad"
        /// </summary>
        /// <param name="mouseState">Objekt för att hämta musens koordinater</param>
        /// <param name="previousMouseState">Objekt som representerar musens tidigare status. Används för att motverka att knappen markeras som klickad när muspekaren tidigare hållts nere utan att släppas</param>
        /// <returns></returns>
        public Texture2D GetActiveAsset(MouseState mouseState, MouseState previousMouseState)
        {
            if (getButtonState(mouseState, previousMouseState) == ButtonStates.Idle)
            {
                ActiveAsset = AssociatedAssets.Item1;
            }
            else
            {
                ActiveAsset = AssociatedAssets.Item2;
            }
            return ActiveAsset;
        }

        /// <summary>
        /// Funktion för att kontrollera om en knapp har klickats på eller inte.
        /// </summary>
        /// <param name="mouseState">Objekt för att hämta musens koordinater</param>
        /// <param name="previousMouseState">Objekt som representerar musens tidigare status. Används för att motverka att knappen markeras som klickad när muspekaren tidigare hållts nere utan att släppas</param>
        /// <returns></returns>
        public bool IsClicked(MouseState mouseState, MouseState previousMouseState)
        {
            if (getButtonState(mouseState, previousMouseState) == ButtonStates.Clicked)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
