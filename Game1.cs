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
        List<PowerUp> currentPowerUps = new List<PowerUp>(); // Lista med de nuvarande inladdade (fångbara) powerupsen i spelet
        List<PowerUp> currentActivatedPowerUps = new List<PowerUp>(); // Lista med de nuvarande aktiva (fångade) powerupsen i spelen
        List<BackgroundImage> _currentFishingBackgroundItems = new List<BackgroundImage>(); // Saker i bakgrunden som visas när man fiskar
        List<FishData> _gameFishies; // Fiskar som är möjligt att spawna
        List<BackgroundImageData> _fishingBackgroundItems; // Namn på de olika bakgrundsföremålen som kan flyta runt på skärmen.
        List<BackgroundImage> _activeBackgroundImages; // Aktiva bakgrundsbilder i spelet.
        List<PowerUpData> _gamePowerUps; // Möjliga powerups att spawna
        Fisherman _fisherman; // Objektet knutet till spelets fiskar
        FishingRod _fishingRod; // Objektet knutet till fiskespöet
        int _score; // Användarens aktuella poäng
        float _depth; // Hur djupt fiskespöet har gått.
        int _maxDepth; // Hur djupt användaren max kan gå ner

        // Texturer
        private Texture2D _fishermanImage;
        private List<Texture2D> _fishermanImages;
        const int FisherManAnimationsCount = 5; // Varje animationsframe för fiskaren slutar på ett nummer. Denna variabel används för att styra hur många animationsrutor "frames" som ska laddas in i spelet.
        private SpriteFont _mainFont; // Standardfontet att använda

        // Multipliers (för powerups)
        double scoreMultiplier = 1;
        double fishMultiplier = 1;
        double cooldownMultiplier = 1;
        double depthMultiplier = 1;
        double speedMultiplier = 1;

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
            _gamePowerUps = new List<PowerUpData>()
            {
                new PowerUpData(
                    name: "Silver sömnpiller",
                    associatedAssetName: "clock-silver",
                    type: PowerUpData.PowerUpTypes.SleepPill,
                    rarity: 10,
                    multiplier: 0.2,
                    activeFor: 30,
                    speed: 1
                ),
                new PowerUpData(
                    name: "Guldigt sömnpiller",
                    associatedAssetName: "clock-silver",
                    type: PowerUpData.PowerUpTypes.SleepPill,
                    rarity: 10,
                    multiplier: 0.5,
                    activeFor: 30,
                    minDepth: 20,
                    speed: 1
                )
            };

            _fishingBackgroundItems = new List<BackgroundImageData>
            {
                new BackgroundImageData("Vattenbubbla", "bubble_small", 1, 10)
                //new BackgroundImageData("Sjögräs #1", "placeholder", 1, 5),
                //new BackgroundImageData("Sjögräs #2", "placeholder", 1, 5),
                //new BackgroundImageData("Sjögräs #3", "placeholder", 1, 5)
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
            // Knappstorlek: 128x64. Därmed kan vi ta fram positionen för att centrera knapparna
            Vector2 buttonCenterPosition = new Vector2(
                (_graphics.PreferredBackBufferWidth / 2) - 64,
                (_graphics.PreferredBackBufferHeight / 2) - 32
            );
            _gameButtons = new Dictionary<string, Button>()
            {
                ["play"] = new Button(
                    "button-start",
                    Content,
                    new Vector2(buttonCenterPosition.X, buttonCenterPosition.Y),
                    GameState.TitleScreen
                ),
                ["help"] = new Button(
                    "button-help",
                    Content,
                    new Vector2(buttonCenterPosition.X, buttonCenterPosition.Y + 74), // Knappstorlek: 128x64
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
                            _fishingRod.Position = new Vector2(0, 0); // Flytta tillbaka fiskespöet till startpositonen
                        }
                        if (
                            _fishingRod.Cooldown
                            <= (gameTime.TotalGameTime.TotalMilliseconds - fishingRodCooldown)
                        )
                        {
                            Debug.WriteLine("Återställer fiskespö...");
                            _fishingRod.MarkAsNotCollidedWith();
                            _fishingRod.Position = new Vector2(0, 0); // Flytta tillbaka fiskespöet till startpositonen
                            _depth = 0; // Återställ aktuellt djup
                        }
                        else
                        {
                            Debug.WriteLine(
                                $"Fiskespöet är fortfarande på cooldown... {gameTime.TotalGameTime.TotalMilliseconds - fishingRodCooldown}/{_fishingRod.Cooldown} millisekunder."
                            );
                        }
                    }
                }
                // Hantera powerups. Se till att de som ska vara aktiverade är det.
                foreach (PowerUp powerUp in currentActivatedPowerUps)
                {
                    if (!powerUp.IsActivated && powerUp.HasBeenCollidedWith) // Om powerupen har kolliderats med av fiskespöet men inte aktiverats så vill vi ju aktivera den. Det gör vi här!
                    {
                        powerUp.activatePowerUp(gameTime.TotalGameTime.TotalSeconds);
                        if (powerUp.Data.Type == PowerUpData.PowerUpTypes.SleepPill)
                        {
                            powerUp.savePreviousValue(speedMultiplier);
                        }
                        else if (powerUp.Data.Type == PowerUpData.PowerUpTypes.Healer)
                        {
                            powerUp.savePreviousValue(cooldownMultiplier);
                        }
                        if (powerUp.Data.Type == PowerUpData.PowerUpTypes.FishMultiplier)
                        {
                            powerUp.savePreviousValue(fishMultiplier);
                        }
                        if (powerUp.Data.Type == PowerUpData.PowerUpTypes.ScoreMultiplier)
                        {
                            powerUp.savePreviousValue(scoreMultiplier);
                        }
                    }
                    if (powerUp.CheckPowerUpStillActive(gameTime.TotalGameTime.TotalSeconds)) // Kontrollera om powerupen fortfarande är aktiverad.
                    {
                        // Isåfall, se till att spelet uppdateras så att man drar fördel av powerupsen
                        // Information: Se klassen "PowerUpData" för mer information om powerups :))
                        if (powerUp.Data.Type == PowerUpData.PowerUpTypes.SleepPill)
                        {
                            speedMultiplier = speedMultiplier + powerUp.Data.Multiplier;
                        }
                        else if (powerUp.Data.Type == PowerUpData.PowerUpTypes.Healer)
                        {
                            cooldownMultiplier = cooldownMultiplier + powerUp.Data.Multiplier;
                        }
                        else if (powerUp.Data.Type == PowerUpData.PowerUpTypes.FishMultiplier)
                        {
                            fishMultiplier = fishMultiplier + powerUp.Data.Multiplier;
                        }
                        else if (powerUp.Data.Type == PowerUpData.PowerUpTypes.ScoreMultiplier)
                        {
                            scoreMultiplier = scoreMultiplier + powerUp.Data.Multiplier;
                        }
                    }
                    else
                    {
                        // Återställ spelets status när powerups blir inaktiva
                        // Information: Se klassen "PowerUpData" för mer information om powerups :))
                        if (powerUp.Data.Type == PowerUpData.PowerUpTypes.SleepPill)
                        {
                            speedMultiplier = powerUp.PreviousValue;
                        }
                        else if (powerUp.Data.Type == PowerUpData.PowerUpTypes.Healer)
                        {
                            cooldownMultiplier = powerUp.PreviousValue;
                        }
                        else if (powerUp.Data.Type == PowerUpData.PowerUpTypes.FishMultiplier)
                        {
                            fishMultiplier = powerUp.PreviousValue;
                        }
                        else if (powerUp.Data.Type == PowerUpData.PowerUpTypes.ScoreMultiplier)
                        {
                            scoreMultiplier = powerUp.PreviousValue;
                        }
                    }
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightBlue);
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
                drawScaled(_fishermanImage, new Vector2(0, 0), 3);
                // Lägg till en instruerande text
                _spriteBatch.DrawString(
                    _mainFont,
                    "Tryck på SPACE för att fånga fisk",
                    new Vector2(
                        (_graphics.PreferredBackBufferWidth) / 2
                            - _mainFont.MeasureString("Tryck på SPACE för att fånga fisk").X / 2,
                        _graphics.PreferredBackBufferHeight / 2
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
                drawScaled(_fishermanImage, new Vector2(0, 0), 3);
            }
            else if (_state == GameState.FishCatchingScreen)
            {
                // Generera bakgrundsfärg baserat på djup
                Color currentBackgroundColor = Color.DarkBlue; //new Color(new Vector3(143, 180, 255));
                GraphicsDevice.Clear(currentBackgroundColor);
                // Skapa fiskar. Målet är att ha 15 fiskar som visas på skärmen samtidigt.
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
                                        + (int)_fishingRod.AssociatedAsset.Height, // (int)_fishingRod.AssociatedAsset.Height / 2
                                    _graphics.PreferredBackBufferHeight
                                )
                        ), // Börja på en slumpvis höjd
                        newFishData,
                        Content
                    );
                    _currentFishies.Add(newFish);
                }
                ;
                // Skapa också föremål i bakgrunden för att skapa en levande bakgrund
                int backgroundImagesToCreate = 15 - _currentFishingBackgroundItems.Count;
                List<BackgroundImageData> availableBackgroundsImages = _fishingBackgroundItems
                    .Where(backgroundImage => backgroundImage.IsAvailableAt(_depth))
                    .ToList();
                for (int i = 0; i < backgroundImagesToCreate; i++)
                {
                    BackgroundImageData randomizedBackgroundImage = availableBackgroundsImages[
                        _random.Next(0, availableBackgroundsImages.Count - 1)
                    ];
                    _currentFishingBackgroundItems.Add(
                        new BackgroundImage(
                            new Vector2(
                                _graphics.PreferredBackBufferWidth
                                    - _random.Next(0, _graphics.PreferredBackBufferWidth),
                                _random.Next(_graphics.PreferredBackBufferHeight)
                            ),
                            Content,
                            randomizedBackgroundImage
                        )
                    );
                }
                //..gör detsamma med powerups (alltså skapa powerups)
                if (_random.Next(1, 1) == 20) // Chansen är 1/20 (5%) att en powerup dyker upp
                {
                    List<PowerUpData> possiblePowerUps = _gamePowerUps
                        .Where(powerup => powerup.IsAvailableAt(_depth))
                        .ToList();
                    PowerUpData randomPowerUp = possiblePowerUps[
                        _random.Next(0, possiblePowerUps.Count - 1)
                    ];
                    Debug.WriteLine("Spawnar en powerup...");
                    currentPowerUps.Add( // Skapa en ny powerup och lägg till den i listan
                        new PowerUp(
                            new Vector2(
                                _graphics.PreferredBackBufferWidth,
                                _graphics.PreferredBackBufferHeight
                                    - _random.Next(
                                        (int)_fishingRod.Position.Y
                                            + (int)_fishingRod.AssociatedAsset.Height,
                                        _graphics.PreferredBackBufferHeight
                                    )
                            ),
                            randomPowerUp,
                            Content
                        )
                    );
                }

                // Rita ut bakgrundsbildsinnehåll
                List<BackgroundImage> _tempCurrentBackgroundImages = new List<BackgroundImage>(
                    _currentFishingBackgroundItems
                );
                foreach (BackgroundImage image in _currentFishingBackgroundItems)
                {
                    Vector2 nextPos = image.getNextPos(
                        _graphics.PreferredBackBufferHeight,
                        _graphics.PreferredBackBufferWidth,
                        0,
                        0
                    );
                    if (image.Position.X == -1) // Negativa koordinater - bakgrundsbilden ska gömmas från skärmen
                    {
                        _tempCurrentBackgroundImages.Remove(image);
                    }
                    else
                    {
                        _spriteBatch.Draw(image.AssociatedAsset, nextPos, Color.White);
                    }
                }
                _currentFishingBackgroundItems = _tempCurrentBackgroundImages; // Uppdatera från temporär lista

                // Rita ut alla fiskar och powerups
                List<Fish> _tempCurrentFishies = new List<Fish>(_currentFishies);
                foreach (Fish fish in _currentFishies)
                {
                    // Kontrollera om fisken kolliderar med metspöet och metspöet inte är "upptaget" med att ta hand om en annan fisk
                    fish.CheckAndHandleCollisionWithFishingRod(_fishingRod, _depth, _graphics);

                    if (fish.Position.X == -1) // Negativa koordinater - fisken ska gömmas från skärmen
                    {
                        _tempCurrentFishies.Remove(fish);
                    }
                    else
                    {
                        _spriteBatch.Draw(fish.AssociatedAsset, fish.Position, Color.White);
                    }
                }
                _currentFishies = _tempCurrentFishies; // Uppdatera lista från temporär lista

                List<PowerUp> _tempCurrentPowerUps = new List<PowerUp>();
                foreach (PowerUp powerUp in currentPowerUps)
                {
                    // Hantera kollision med powerupen
                    powerUp.CheckAndHandleCollisionWithFishingRod(_fishingRod, _depth, _graphics);
                    if (powerUp.HasBeenCollidedWith && !currentActivatedPowerUps.Contains(powerUp)) // Lägg till powerupen i listan med powerups när den har aktiveras
                    {
                        currentActivatedPowerUps.Add(powerUp);
                    }
                    if (powerUp.Position.X == -1) // Negativa koordinater - powerupen ska gömmas från skärmen
                    {
                        _tempCurrentPowerUps.Remove(powerUp);
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

                // Skriv ut gränsen till nästa djup
                _spriteBatch.DrawString(
                    _mainFont,
                    $"Till nästa nivå: -- poäng",
                    new Vector2(_graphics.PreferredBackBufferWidth - 150, 75),
                    Color.White
                );

                // Om fiskespöet är nära maxdjupet, rita ut en text
                Vector2 lockedStringTextSize = _mainFont.MeasureString("---LÅST--");
                if (_graphics.PreferredBackBufferHeight - _depth <= lockedStringTextSize.Y)
                { //(_depth-variabeln är i antal pixlar)
                    _spriteBatch.DrawString(
                        _mainFont,
                        $"---LÅST---",
                        new Vector2(
                            _graphics.PreferredBackBufferWidth - (lockedStringTextSize.X / 2),
                            _graphics.PreferredBackBufferHeight - (lockedStringTextSize.Y / 2)
                        ),
                        Color.White
                    );
                }
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

        /// <summary>
        /// Något som är lite irriteraned med MonoGame är att man inte kan skala upp en bild som man skapat utan att skriva en jättelång "call" till .Draw().
        /// Jag skapar här en funktion som stödjer att rita ut saker skalade.
        /// </summary>
        /// <param name="image">Bilden/texturen som ska ritas ut.</param>
        /// <param name="position">Texturens position på skärmen</param>
        /// <param name="scaleFactor">Hur många gånger texturen ska föstoras "skalas upp".</param>
        public void drawScaled(Texture2D image, Vector2 position, float scaleFactor)
        {
            _spriteBatch.Draw(
                image,
                position,
                null,
                Color.White,
                0,
                new Vector2(0, 0),
                scaleFactor,
                SpriteEffects.None,
                0
            );
        }
    }
}
