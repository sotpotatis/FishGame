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
        HelpScreen, // Skärmen man kan använda för att få hjälp om spelet
        IdleScreen, // Skärmen som visas när man inte fångar fiskar
        IdleScreenAnimating, // Skärmen som visas när fiskespöet kastas ut
        FishCatchingScreen // Skärmen som visas när man fångar fiskar
    };

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameState _state;

        // Data av olika slag
        List<Fish> _currentFishies = new List<Fish>(); // Lista med de nuvarande inladdade fiskarna i spelet
        List<PowerUp> currentPowerUps = new List<PowerUp>(); // Lista med de nuvarande inladdede powerupsen i spelet
        List<FishData> _gameFishies;
        Fisherman _fisherman;
        FishingRod _fishingRod;
        int _score; // Användarens aktuella poäng
        float _depth; // Hur djupt fiskespöet har gått.
        int _maxDepth; // Hur djupt användaren max kan gå ner

        // Texturer
        private Texture2D _fishermanImage;
        private List<Texture2D> _fishermanImages;
        const int FisherManAnimationsCount = 5; // Varje animationsframe för fiskaren slutar på ett nummer. Denna variabel används för att styra hur många animationsrutor "frames" som ska laddas in i spelet.
        private SpriteFont _mainFont; // Standardfontet att använda

        // Övrigt
        Random _random = new Random();
        MouseState _previousMouseState;
        Dictionary<string, Button> _gameButtons; // Knappar som spelet innehåller.
        double _timeSinceLastAnimation;
        double fishingRodCooldown;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            // _graphics.IsFullScreen = true; // Specificera att spelet ska köras i fullskärm
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _state = GameState.TitleScreen;
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
            // Nedan följer detsamma, fast för powerups
            PowerUpData[] _gamePowerUps = new PowerUpData[]
            {
                new PowerUpData(
                    name: "Silver sömnpiller",
                    associatedAssetName: "placeholder",
                    rarity: 10,
                    activeFor: 30
                ),
                new PowerUpData(
                    name: "Guldigt sömnpiller",
                    associatedAssetName: "placeholder",
                    rarity: 10,
                    activeFor: 30,
                    minDepth: 20
                )
            };
            // Initiera också fiskaren
            _fisherman = new Fisherman();
            // ... fiskespöet...
            _fishingRod = new FishingRod(
                new Vector2(0, 0),
                5,
                10,
                2000,
                "fishing-rod-placeholder",
                Content
            );
            //...lite knappar i spelet...
            _gameButtons = new Dictionary<string, Button>()
            {
                ["play"] = new Button(
                    "button-start",
                    Content,
                    new Vector2(_graphics.PreferredBackBufferWidth/2, _graphics.PreferredBackBufferHeight / 2),
                    GameState.TitleScreen
                ),
                ["help"] = new Button(
                    "button-help",
                    Content,
                    new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2 +120),
                    GameState.TitleScreen
                ),
            };
            //...och ställ in djupet samt andra variabler
            _depth = 0;
            _score = 0;
            _maxDepth = 500;
            _timeSinceLastAnimation = 0;
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
            // Hantera och möjliggör att stoppa spelet
            if (
                GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape)
            )
            {
                Exit();
            }

            MouseState _mouseState = Mouse.GetState();
            // Uppdatera alla knappars bilder baserade på om musen är över de eller inte (detta händer endast i minnet, relevanta knappar ritas ut under Draw)
            Dictionary<string, Button> _tempGameButtons = new Dictionary<string, Button>(
                _gameButtons
            ); // Skapa temporär dictionary
            foreach (KeyValuePair<string, Button> gameButton in _gameButtons)
            {
                if (gameButton.Value.ActiveOnScreen == _state)
                {
                    gameButton.Value.ActiveAsset = gameButton.Value.GetActiveAsset(
                        _mouseState,
                        _previousMouseState
                    );
                    _tempGameButtons[gameButton.Key] = gameButton.Value;
                }
            }
            _gameButtons = _tempGameButtons;

            if (_state == GameState.TitleScreen)
            {
                // Kontrollera om någon av titelskärmens knappar har klickats på
                if (_gameButtons["play"].IsClicked(_mouseState, _previousMouseState))
                { // Byt från titelskärmen till spelskärmen
                    _state = GameState.IdleScreen;
                }
                else if (_gameButtons["help"].IsClicked(_mouseState, _previousMouseState))
                { // Byt från titelskärmen till hjälpskärmen
                    _state = GameState.HelpScreen;
                }
            }
            else if (_state == GameState.IdleScreen)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    // Man kastar ut fiskespöet genom att klicka på mellanslag
                    _state = GameState.IdleScreenAnimating;
                }
            }
            else if (_state == GameState.FishCatchingScreen)
            {
                if (!_fishingRod.HasBeenCollidedWith)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.Up))
                    {
                        // Flytta upp om vi kan det
                        if (_depth > 0)
                        {
                            if (_depth - _fishingRod.Speed <= 0)
                            {
                                _depth = 0;
                            }
                            else
                            {
                                _depth -= _fishingRod.Speed;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Y-gräns för metspöet uppåt är nådd.");
                        }
                    }
                    else if (Keyboard.GetState().IsKeyDown(Keys.Down))
                    {
                        // Flytta nedåt om vi kan det
                        if (_depth + _fishingRod.Speed < _maxDepth)
                        {
                            _depth += _fishingRod.Speed;
                        }
                        else
                        {
                            Debug.WriteLine("Y-gräns för metspöet nedåt är nådd.");
                        }
                    }
                    else if (Keyboard.GetState().IsKeyDown(Keys.Right))
                    {
                        // Flytta metspöet åt höger om vi kan det
                        if (
                            _fishingRod.Position.X + _fishingRod.AssociatedAsset.Width
                            <= _graphics.PreferredBackBufferWidth
                        )
                        {
                            _fishingRod.Position = new Vector2(
                                _fishingRod.Position.X + _fishingRod.SpeedX,
                                _fishingRod.Position.Y
                            );
                        }
                        else
                        {
                            Debug.WriteLine("X-gräns för metspöet (höger) är nådd.");
                        }
                    }
                    else if (Keyboard.GetState().IsKeyDown(Keys.Left))
                    { // Flytta metspöet åt vänster om vi kan det
                        if (_fishingRod.Position.X - _fishingRod.AssociatedAsset.Width >= 0)
                        {
                            _fishingRod.Position = new Vector2(
                                _fishingRod.Position.X - _fishingRod.SpeedX,
                                _fishingRod.Position.Y
                            );
                        }
                        else
                        {
                            Debug.WriteLine("X-gräns för metspöet (vänster) är nådd.");
                        }
                    }
                }
                else
                {
                    // Hantera uppfiskning. Om fisken har fiskats upp så vill vi att fiskespöet ska vara tillgänglig till att fånga mer fisk
                    if (_fishingRod.HasBeenCollidedWith)
                    {
                        if (fishingRodCooldown == 0)
                        { // Värdet är 0 när cooldownen inte har startat än. Det återställs av koden när kollisionen inleds
                            fishingRodCooldown = gameTime.TotalGameTime.TotalMilliseconds;
                        }
                        if (
                            _fishingRod.Cooldown
                            <= (gameTime.TotalGameTime.TotalMilliseconds - fishingRodCooldown)
                        )
                        {
                            Debug.WriteLine("Återställer fiskespö...");
                            _fishingRod.MarkAsNotCollidedWith();
                        }
                        else
                        {
                            Debug.WriteLine(
                                $"Fiskespöet är fortfarande på cooldown... {fishingRodCooldown}/{_fishingRod.Cooldown} millisekunder."
                            );
                        }
                    }
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            // Rita ut alla knappar. Dessa sköts av min egna knapphanterare - magiskt va?
            foreach (Button button in _gameButtons.Values)
            {
                if (button.ActiveOnScreen == _state)
                { //Om knappen ska visas på den aktuella skärmen, rita ut den
                    _spriteBatch.Draw(button.ActiveAsset, button.Position, Color.White);
                }
            }
            if (_state == GameState.TitleScreen)
            {
                // Kod för titelskärmen
                // Rita ut de objekt som ska vara på titelskärmen


            }
            else if (_state == GameState.IdleScreen)
            {
                // Visa bilden på fiskaren
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
                // Lägg till en instruerande text
                _spriteBatch.DrawString(
                    _mainFont,
                    "Tryck på SPACE för att fånga fisk",
                    new Vector2(
                        _graphics.PreferredBackBufferWidth
                            - _mainFont.MeasureString("Tryck på SPACE för att fånga fisk").X / 2,
                        300
                    ),
                    Color.White
                );
                // TODO: Lägg till statistiktext etc.
            }
            else if (_state == GameState.IdleScreenAnimating)
            {
                // Ladda in fiskaren
                // Hämta nästa bild i animationen om vi just nu animerar
                int _fishermanImageIndex = _fishermanImages.IndexOf(_fishermanImage);
                if (gameTime.TotalGameTime.TotalMilliseconds - _timeSinceLastAnimation > 1000)
                {
                    if (_fishermanImageIndex + 1 < _fishermanImages.Count)
                    {
                        _fishermanImageIndex += 1;
                    }
                    else
                    { // Om vi har animerat klart, byt sedan skärm
                        _state = GameState.FishCatchingScreen; // Byt skärm om vi har animerat klart
                    }
                    Debug.WriteLine($"Uppdaterar: nytt index är {_fishermanImageIndex}");
                    _timeSinceLastAnimation = gameTime.TotalGameTime.TotalMilliseconds;
                }
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
            else if (_state == GameState.FishCatchingScreen)
            {
                // Generera bakgrundsfärg baserat på djup
                Color currentBackgroundColor = Color.DarkBlue; //new Color(new Vector3(143, 180, 255));
                GraphicsDevice.Clear(currentBackgroundColor);
                // Rita ut fiskar. Målet är att ha 15 fiskar som visas på skärmen samtidigt.
                // Kontrollera vilka fiskar som är möjliga för det djupet vi har
                List<FishData> _possibleFishes = _gameFishies
                    .Where(fish => fish.IsAvailableAt(getDepthInMetres()))
                    .ToList();
                Debug.WriteLine($"{_possibleFishes.Count} fiskar tillgängliga");
                int fishesToCreate = 15 - _currentFishies.Count;
                for (int i = 0; i < fishesToCreate; i++)
                {
                    Debug.WriteLine($"Skapar en ny fisk... ({i + 1}/{fishesToCreate})");
                    FishData newFishData = _possibleFishes[
                        _random.Next(0, _possibleFishes.Count - 1)
                    ];
                    Fish newFish = new Fish(
                        new Vector2(
                            _graphics.PreferredBackBufferWidth,
                            _graphics.PreferredBackBufferHeight
                                - _random.Next(
                                    (int)_fishingRod.Position.Y
                                        + (int)_fishingRod.AssociatedAsset.Height / 2,
                                    _graphics.PreferredBackBufferHeight
                                )
                        ), // Börja på en slumpvis höjd
                        newFishData,
                        Content
                    );
                    _currentFishies.Add(newFish);
                }
                ;
                // Rita ut alla fiskar och powerups
                List<Fish> _tempCurrentFishies = new List<Fish>(_currentFishies);
                foreach (Fish fish in _currentFishies)
                {
                    // Kontrollera om fisken kolliderar med metspöet och metspöet inte är "upptaget" med att ta hand om en annan fisk
                    fish.CheckAndHandleCollisionWithFishingRod(_fishingRod, _depth, _graphics);

                    if (fish.Position.X == -1) // Negativa koordinater - fisken ska gömmas från skärmen
                    {
                        _tempCurrentFishies.Remove(fish);
                        fish.MarkAsNotCollidedWith();
                        fishingRodCooldown = 0;
                    }
                    else
                    {
                        _spriteBatch.Draw(fish.AssociatedAsset, fish.Position, Color.White);
                    }
                }
                _currentFishies = _tempCurrentFishies; // Uppdatera lista från temporär lista

                List<PowerUp> _tempCurrentPowerUps = new List<PowerUp>(currentPowerUps);
                foreach (PowerUp powerUp in currentPowerUps)
                {
                    // Kontrollera om fisken kolliderar med metspöet och metspöet inte är "upptaget" med att ta hand om en annan fisk
                    powerUp.CheckAndHandleCollisionWithFishingRod(_fishingRod, _depth, _graphics);

                    if (powerUp.Position.X == -1) // Negativa koordinater - fisken ska gömmas från skärmen
                    {
                        _tempCurrentPowerUps.Remove(powerUp);
                        powerUp.MarkAsNotCollidedWith();
                        fishingRodCooldown = 0;
                    }
                    else
                    {
                        _spriteBatch.Draw(powerUp.AssociatedAsset, powerUp.Position, Color.White);
                    }
                }
                currentPowerUps = _tempCurrentPowerUps; // Uppdatera lista från temporär lista

                // Rita ut kroken/metspöet/fiskespöet
                _spriteBatch.Draw(
                    _fishingRod.AssociatedAsset,
                    _fishingRod.getNextPos(0, 0, 0, 0),
                    Color.White
                );

                // Rita ut text som visar aktuellt djup, poäng, etc
                double _depthInMetres = getDepthInMetres();
                _spriteBatch.DrawString(
                    _mainFont,
                    $"Poäng: {_score}",
                    new Vector2(_graphics.PreferredBackBufferWidth - 150, 25),
                    Color.White
                );
                _spriteBatch.DrawString(
                    _mainFont,
                    $"Djup: {_depthInMetres} m",
                    new Vector2(_graphics.PreferredBackBufferWidth - 150, 50),
                    Color.White
                );
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Eftersom _depth-variabeln är i pixlar så finns det en funktion för att konvertera den till meter i havet som är mer användarvänligt.
        /// </summary>
        /// <returns></returns>
        public double getDepthInMetres()
        {
            return Math.Round(_depth / (_graphics.PreferredBackBufferHeight / 8), 1); // 1 m är 1/8 av skärmen
        }
    }
}
