﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FishGame
{
    enum GameState
    { // Innehåller olika händelser i spelet som påverkar vad som visas på skärmen.
        TitleScreen, // Titelskärmen på spelet
        IdleScreen, // Skärmen som visas när man inte fångar fiskar
        FishCatchingScreen // Skärmen som visas när man fångar fiskar
    };

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameState _state;

        // Data av olika slag
        List<Fish> _currentFishies = new List<Fish>(); // Lista med de nuvarande inladdade fiskarna i spelet
        FishData[] _gameFishies;
        Fisherman _fisherman;
        FishingRod _fishingRod;
        int _score; // Användarens aktuella poäng
        int _depth; // Hur djupt fiskespöet har gått.

        // Texturer
        private Texture2D _fishermanImage;
        private List<Texture2D> _fishermanImages;
        const int FisherManAnimationsCount = 5; // Varje animationsframe för fiskaren slutar på ett numer

        Random _random = new Random();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            // _graphics.IsFullScreen = true; // Specificera att spelet ska köras i fullskärm
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _state = GameState.IdleScreen;
        }

        protected override void Initialize()
        {
            // Nedan följer en lista med alla fiskar som finns i spelet och deras standarddata.
            _gameFishies = new FishData[]
            {
                new FishData(
                    name: "Blå Fisk",
                    associatedAssetName: "blue_fishie",
                    rarity: 1,
                    value: 1,
                    defaultSpeed: 5
                ),
                new FishData(
                    name: "Grön Fisk",
                    associatedAssetName: "green_fishie",
                    rarity: 1,
                    value: 1,
                    defaultSpeed: 5
                ),
                new FishData(
                    name: "Röd Fisk",
                    associatedAssetName: "red_fishie",
                    rarity: 1,
                    value: 1,
                    defaultSpeed: 5
                ),
                new FishData(
                    name: "Stor Fisk",
                    associatedAssetName: "big_fishie",
                    rarity: 2,
                    value: 10,
                    defaultSpeed: 8
                ),
                new FishData(
                    name: "Lila Fisk",
                    associatedAssetName: "purple_blurple",
                    rarity: 2,
                    value: 3,
                    defaultSpeed: 10
                ),
                new FishData(
                    name: "Guldig Sällsynt Fisk",
                    associatedAssetName: "golden_fancy_fish",
                    rarity: 5,
                    value: 20,
                    defaultSpeed: 20
                ),
            };
            // Nedan följer detsamma, fast för skräp
            JunkData[] _gameJunk = new JunkData[]
            {
                new JunkData(
                    name: "Stövel",
                    associatedAssetName: "placeholder",
                    rarity: 10,
                    value: -10
                )
            };
            // Initiera också fiskaren
            _fisherman = new Fisherman();
            //...och ställ in djupet samt andra variabler
            _depth = 0;
            _score = 0;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            // Fiskaren är animerad, så det finns fler bilder på honom. Därför laddar vi in allihopa.
            _fishermanImages = new List<Texture2D>();
            for (int i = 0; i < FisherManAnimationsCount; i++)
            {
                _fishermanImages.Add(Content.Load<Texture2D>($"fisherman-{i}"));
                Debug.WriteLine($"Lade till fiskarbild {i + 1}.");
            }
            ;
            _fishermanImage = _fishermanImages[0]; // Använd den första bilden i animationen
            _fisherman.Asset = _fishermanImage;
        }

        protected override void Update(GameTime gameTime)
        {
            // Hantera och möjliggör att stoppa spelet samt att flytta på fiskespöet.
            if (
                GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape)
            )
                Exit();

            if (_state == GameState.FishCatchingScreen)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Up)) { }
                else if (Keyboard.GetState().IsKeyDown(Keys.Down)) { }
                else if (Keyboard.GetState().IsKeyDown(Keys.Right)) { }
                else if (Keyboard.GetState().IsKeyDown(Keys.Left)) { }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            if (_state == GameState.IdleScreen)
            {
                // Ladda in fiskaren
                // Hämta nästa bild i animationen om vi just nu animerar
                int _fishermanImageIndex = _fishermanImages.IndexOf(_fishermanImage) + 1;
                if (_fishermanImageIndex + 1 < _fishermanImages.Count)
                {
                    if (Math.Round(gameTime.TotalGameTime.TotalMilliseconds) % 1000 == 0)
                    { // Byt endast animationen varannan sekund
                        _fishermanImageIndex += 1;
                        _fishermanImage = _fishermanImages[_fishermanImageIndex];
                        _spriteBatch.Draw(
                            _fishermanImage,
                            new Vector2(0, 0),
                            null,
                            Color.White,
                            0,
                            new Vector2(0, 0),
                            1.5f,
                            SpriteEffects.None,
                            0
                        );
                    }
                }
                else
                { // Om vi har animerat klart, flytta upp fiskaren och byt sedan skärm
                    if (_fisherman.Position.Y - _fisherman.Asset.Height <= 0)
                    {
                        _state = GameState.FishCatchingScreen; // Byt skärm om vi har animerat klart
                    }
                    else
                    { // Flytta upp fiskaren ut från skärmen
                        _fisherman.Position = new Vector2(
                            _fisherman.Position.X,
                            _fisherman.Position.Y - 1
                        );
                    }
                }
            }
            else if (_state == GameState.FishCatchingScreen)
            {
                // Ladda in fiskar. Målet är att ha 15 fiskar som visas på skärmen samtidigt.
                int fishesToCreate = 15 - _currentFishies.Count;
                for (int i = 0; i < fishesToCreate; i++)
                {
                    Debug.WriteLine($"Skapar en ny fisk... ({i + 1}/{fishesToCreate})");
                    FishData newFishData = _gameFishies[_random.Next(0, _gameFishies.Length - 1)];
                    Fish newFish = new Fish(
                        new Vector2(
                            _graphics.PreferredBackBufferWidth,
                            _graphics.PreferredBackBufferHeight - _random.Next(100, 300)
                        ), // Börja vid skärmens kant
                        newFishData,
                        Content
                    );
                    _currentFishies.Add(newFish);
                }
                ;
                // Rita ut alla fiskar
                List<Fish> _tempCurrentFishies = new List<Fish>(_currentFishies);
                foreach (Fish fish in _currentFishies)
                {
                    Vector2 nextFishPos = fish.getNextPos(
                        _graphics.PreferredBackBufferHeight,
                        _graphics.PreferredBackBufferWidth
                    );
                    if (nextFishPos.X == -1) // Negativa koordinater - fislen ska gömmas från skärmen
                    {
                        _tempCurrentFishies.Remove(fish);
                    }
                    else
                    {
                        _spriteBatch.Draw(fish.AssociatedAsset, nextFishPos, Color.White);
                    }
                }
                // Rita ut kroken/metspöet

                _currentFishies = _tempCurrentFishies; // Uppdatera lista från temporär lista
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}