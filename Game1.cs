using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        List<FishData> _gameFishies;
        Fisherman _fisherman;
        FishingRod _fishingRod;
        int _score; // Användarens aktuella poäng
        float _depth; // Hur djupt fiskespöet har gått.
        int _maxDepth; // Hur djupt användaren max kan gå ner
        const float DepthToMetersFactor = 1; // Divisionsvärde för att konvertera _depth (som är i pixlar) till meter.

        // Texturer
        private Texture2D _fishermanImage;
        private List<Texture2D> _fishermanImages;
        const int FisherManAnimationsCount = 5; // Varje animationsframe för fiskaren slutar på ett numer
        private SpriteFont _mainFont; // Standardfontet att använda
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
            _gameFishies = new List<FishData>()
            {
                new FishData(
                    name: "Blå Fisk",
                    associatedAssetName: "blue_fishie",
                    rarity: 1,
                    value: 1,
                    defaultSpeed: 5,
                    minSpawnDepth: 0
                ),
                new FishData(
                    name: "Grön Fisk",
                    associatedAssetName: "green_fishie",
                    rarity: 1,
                    value: 1,
                    defaultSpeed: 5,
                    minSpawnDepth: 0
                ),
                new FishData(
                    name: "Röd Fisk",
                    associatedAssetName: "red_fishie",
                    rarity: 1,
                    value: 1,
                    defaultSpeed: 5,
                    minSpawnDepth: 0

                ),
                new FishData(
                    name: "Stor Fisk",
                    associatedAssetName: "big_fishie",
                    rarity: 2,
                    value: 10,
                    defaultSpeed: 8,
                    minSpawnDepth: 10
                ),
                new FishData(
                    name: "Lila Fisk",
                    associatedAssetName: "purple_blurple",
                    rarity: 2,
                    value: 3,
                    defaultSpeed: 10,
                    minSpawnDepth: 0
                ),
                new FishData(
                    name: "Guldig Sällsynt Fisk",
                    associatedAssetName: "golden_fancy_fish",
                    rarity: 5,
                    value: 20,
                    defaultSpeed: 20,
                    minSpawnDepth: 0

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
            // ... fiskespöet...
            _fishingRod = new FishingRod(new Vector2(0,0),
            5,
            10,
            "fishing-rod-placeholder",
            Content
                );
            //...och ställ in djupet samt andra variabler
            _depth = 0;
            _score = 0;
            _maxDepth = 500;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _mainFont = Content.Load<SpriteFont>("mainFont");
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

            if (_state == GameState.FishCatchingScreen && !_fishingRod.HasBeenCollidedWith)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Up)) {
                // Flytta metspöet upp om vi kan det
                if (_depth > 0)
                    {
                        if (_depth - _fishingRod.Speed <= 0) {
                            _depth = 0;
                        }
                        else
                        {

                        _depth -= _fishingRod.Speed;
                        _fishingRod.Position = new Vector2(_fishingRod.Position.X, _fishingRod.Position.Y-_fishingRod.Speed);
                        }

                    }
                    else
                    {
                        Debug.WriteLine("Y-gräns för metspöet uppåt är nådd.");
                    }
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.Down)) {
                    // Flytta metspöet nedåt om vi kan det
                    if (_depth + _fishingRod.Speed < _maxDepth )
                    {
                        _depth += _fishingRod.Speed;
                        _fishingRod.Position = new Vector2(_fishingRod.Position.X, _fishingRod.Position.Y + _fishingRod.Speed);
                    }
                    else
                    {
                        Debug.WriteLine("Y-gräns för metspöet nedåt är nådd.");
                    }
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.Right)) {
                    // Flytta metspöet åt höger om vi kan det
                    if (_fishingRod.Position.X + _fishingRod.AssociatedAsset.Width <= _graphics.PreferredBackBufferWidth) { 
                        _fishingRod.Position = new Vector2(_fishingRod.Position.X + _fishingRod.SpeedX, _fishingRod.Position.Y);
                    }
                    else
                    {
                        Debug.WriteLine("X-gräns för metspöet (höger) är nådd.");
                    }

                }
                else if (Keyboard.GetState().IsKeyDown(Keys.Left)) { // Flytta metspöet åt vänster om vi kan det
                if (_fishingRod.Position.X - _fishingRod.AssociatedAsset.Width >= 0) {
                    _fishingRod.Position = new Vector2(_fishingRod.Position.X - _fishingRod.SpeedX, _fishingRod.Position.Y);
                }
                else {
                    Debug.WriteLine("X-gräns för metspöet (vänster) är nådd.");
                }}
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            if (_state == GameState.TitleScreen)
            {
                // Kod för titelskärmen
                // Rita ut de objekt som ska vara på titelskärmen


            }
            else if (_state == GameState.IdleScreen)
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
                // Generera bakgrundsfärg baserat på djup
                Color currentBackgroúndColor = new Color(new Vector3(143, 180, 255));
                GraphicsDevice.Clear(currentBackgroúndColor);
                // Rita ut fiskar. Målet är att ha 15 fiskar som visas på skärmen samtidigt.
                // Kontrollera vilka fiskar som är möjliga för det djupet vi har
                List<FishData> _possibleFishes = _gameFishies.Where(fish => fish.IsAvailableAt(_depth)).ToList();
                Debug.WriteLine($"{_possibleFishes.Count} fiskar tillgängliga");
                int fishesToCreate = 15 - _currentFishies.Count;
                for (int i = 0; i < fishesToCreate; i++)
                {
                    Debug.WriteLine($"Skapar en ny fisk... ({i + 1}/{fishesToCreate})");
                    FishData newFishData = _possibleFishes[_random.Next(0, _possibleFishes.Count - 1)];
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
                    // Kontrollera om fisken kolliderar med metspöet och metspöet inte är "upptaget" med att ta hand om en annan fisk
                    if (_fishingRod.HasBeenCollidedWith == false && fish.getAssociatedRectangle().Intersects(_fishingRod.getAssociatedRectangle())) {
                        Debug.WriteLine("Vi har en kollison mellan en fisk och ett metspö.");
                        fish.HasBeenCollidedWith = true; // Markera kollision
                        _fishingRod.HasBeenCollidedWith = true;
                        _fishingRod.CollidedFish = fish; // Ställ in kolliderad fisk
                    };
                    Vector2 nextFishPos = fish.getNextPos(
                        _graphics.PreferredBackBufferHeight,
                        _graphics.PreferredBackBufferWidth,
                        _depth,
                        _fishingRod.Speed
                    );

                    if (nextFishPos.X == -1) // Negativa koordinater - fisken ska gömmas från skärmen
                    {
                        _tempCurrentFishies.Remove(fish);
                        // Om det var en kollision som nyss skedde så gör fiskespöet tillgängligt att fånga mer fisk
                        if (fish.HasBeenCollidedWith && _fishingRod.HasBeenCollidedWith) { 
                        _fishingRod.HasBeenCollidedWith = false;
                        }
                    }
                    else
                    {
                        _spriteBatch.Draw(fish.AssociatedAsset, nextFishPos, Color.White);
                    }
                }
                _currentFishies = _tempCurrentFishies; // Uppdatera lista från temporär lista

                // Rita ut kroken/metspöet/fiskespöet
                _spriteBatch.Draw(_fishingRod.AssociatedAsset, _fishingRod.getNextPos(0,0,0,0), Color.White);

                // Rita ut text som visar aktuellt djup
                double _depthInMetres = Math.Round(_depth / DepthToMetersFactor, 1);
                _spriteBatch.DrawString(_mainFont, $"Djup: {_depthInMetres} m", new Vector2(_graphics.PreferredBackBufferWidth-150, 25), Color.White);
            }
            
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
