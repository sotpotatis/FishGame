using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

/*
 VÄLKOMMEN TILL KÄLLKODEN FÖR FISHY!
Jag vill bara lägga till här att det står i instruktionerna att man ska kunna redogöra för förbättringar och vidareutvecklingar.

Om du tittar i koden nedan så finns det några möjliga vidareutvecklingar:
1. Den första är till och med inlagd men inte implementerad. Det är att fiskarna har olika "raritet", dvs. att vissa fiskar dyker upp oftare än andra.
Detsamma gäller powerups och bakgrundsföremål. Detta finns implementerat i Data-klasserna för dessa, men själva värdet används inte för någonting.
Att olika fiskar ger olika antal poäng är inte heller implementerat.
2. Den andra är mer innehåll. En sak som inte är inlagd än är fler "skins" för fiskespöet med olika egenskaper.
3. Den tredje är att spelet ska kunna fungera i fullskärm. Detta är i princip fullt möjligt idag, förutom att knappar och instruktionskärmen använder en relativ
positionering som gör att det ser lite skumt ut i fullskärm.
 */
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
        List<PowerUp> _currentPowerUps = new List<PowerUp>(); // Lista med de nuvarande inladdade (fångbara) powerupsen i spelet
        List<PowerUp> _currentActivatedPowerUps = new List<PowerUp>(); // Lista med de nuvarande aktiva (fångade) powerupsen i spelen
        List<BackgroundImage> _currentFishingBackgroundItems = new List<BackgroundImage>(); // Saker i bakgrunden som visas när man fiskar
        List<FishData> _gameFishies; // Fiskar som är möjligt att spawna
        List<BackgroundImageData> _gameFishingBackgroundItems; // Namn på de olika bakgrundsföremålen som kan flyta runt på skärmen.
        List<PowerUpData> _gamePowerUps; // Möjliga powerups att spawna
        Fisherman _fisherman; // Objektet knutet till spelets fiskar
        FishingRod _fishingRod; // Objektet knutet till fiskespöet
        int _score; // Användarens aktuella poäng
        int _scoreUntilNextDepth; // Poäng som krävs till nästa djup ska låsas upp
        float _depth; // Hur djupt fiskespöet har gått.
        int _maxDepth; // Hur djupt användaren max kan gå ner
        int _currentLevel; // Användarens aktuella nivå

        // Texturer
        private Texture2D _fishermanImage;
        private List<Texture2D> _fishermanImages;
        const int _fisherManAnimationsCount = 5; // Varje animationsframe för fiskaren slutar på ett nummer. Denna variabel används för att styra hur många animationsrutor "frames" som ska laddas in i spelet.
        private SpriteFont _mainFont; // Standardtypsnittet att använda
        private SpriteFont _mainFontSmall;
        private SpriteFont _titleFont;
        private Texture2D _instructionImage;
        private Texture2D _fishSilhouette;
        private Texture2D _gameLogo;
        private Texture2D _hourGlassImage;
        private Texture2D _mainInstructionImage;
        private Texture2D _mainInstructionImage2;
        private Dictionary<string, Texture2D> _fishImages = new Dictionary<string, Texture2D>();
        private Dictionary<string, Texture2D> _powerUpImages = new Dictionary<string, Texture2D>();
        private Dictionary<string, Texture2D> _fishingBackgroundItems =
            new Dictionary<string, Texture2D>();

        // Multipliers (för powerups)
        double _scoreMultiplier = 1;
        double _fishMultiplier = 1;
        double _cooldownMultiplier = 1;
        double _speedMultiplier = 1;
        int _fishCountTarget = 5;

        // Övrigt
        Random _random = new Random();
        MouseState _previousMouseState;
        Dictionary<string, Button> _gameButtons; // Knappar som spelet innehåller.
        double _timeSinceLastAnimation;
        double _fishingRodCooldown;
        int _lastPowerUpSpawned;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
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
                    associatedAssetName: "blue_fishie_big",
                    rarity: 1,
                    value: 1,
                    defaultSpeed: 5,
                    minSpawnDepth: 0
                ),
                new FishData(
                    name: "Grön Fisk",
                    associatedAssetName: "green_fishie_big",
                    rarity: 1,
                    value: 1,
                    defaultSpeed: 6,
                    minSpawnDepth: 7
                ),
                new FishData(
                    name: "Röd Fisk",
                    associatedAssetName: "red_fishie_big",
                    rarity: 1,
                    value: 1,
                    defaultSpeed: 7,
                    minSpawnDepth: 14
                ),
                new FishData(
                    name: "Stor Fisk",
                    associatedAssetName: "big_fishie_big",
                    rarity: 2,
                    value: 10,
                    defaultSpeed: 8,
                    minSpawnDepth: 20
                ),
                new FishData(
                    name: "Lila Fisk",
                    associatedAssetName: "purple_blurple_big",
                    rarity: 2,
                    value: 3,
                    defaultSpeed: 10,
                    minSpawnDepth: 30
                ),
                new FishData(
                    name: "Guldig Fisk",
                    associatedAssetName: "golden_fancy_fish_big",
                    rarity: 5,
                    value: 20,
                    defaultSpeed: 20,
                    minSpawnDepth: 37
                ),
            };
            // Nedan följer detsamma, fast för powerups
            _gamePowerUps = new List<PowerUpData>()
            {
                new PowerUpData(
                    name: "Silvrig nedsaktare",
                    associatedAssetName: "clock-silver-3",
                    type: PowerUpData.PowerUpTypes.SleepPill,
                    rarity: 10,
                    multiplier: 0.1,
                    activeFor: 30,
                    speed: 10
                ),
                new PowerUpData(
                    name: "Guldig nedsaktare",
                    associatedAssetName: "clock-gold",
                    type: PowerUpData.PowerUpTypes.SleepPill,
                    rarity: 10,
                    multiplier: 0.05,
                    activeFor: 30,
                    minDepth: 0,
                    speed: 20
                ),
                new PowerUpData(
                    name: "2x multiplikator",
                    associatedAssetName: "multiplier-2x",
                    type: PowerUpData.PowerUpTypes.ScoreMultiplier,
                    rarity: 10,
                    multiplier: 2,
                    activeFor: 30,
                    minDepth: 0,
                    speed: 30
                ),
                new PowerUpData(
                    name: "2x fiskmultiplikator",
                    associatedAssetName: "fish-amount-multiplier-2x",
                    type: PowerUpData.PowerUpTypes.FishMultiplier,
                    rarity: 10,
                    multiplier: 2,
                    activeFor: 30,
                    minDepth: 0,
                    speed: 40
                ),
                new PowerUpData(
                    name: "Cooldownhjälpare",
                    associatedAssetName: "fishing-rod-healing",
                    type: PowerUpData.PowerUpTypes.Healer,
                    rarity: 10,
                    multiplier: 2,
                    activeFor: 2,
                    minDepth: 20,
                    speed: 20
                )
            };

            _gameFishingBackgroundItems = new List<BackgroundImageData>
            {
                new BackgroundImageData("Vattenbubbla", "bubble_small", 1, 20)
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
            // Knappstorlek: 128x64. Därmed kan vi ta fram positionen för att centrera knapparna (om de ritas upp i 2x)
            int buttonCenterX = (_graphics.PreferredBackBufferWidth / 2) - 64 * 2;
            int buttonCenterY = (_graphics.PreferredBackBufferHeight / 2) - 32 * 2 + 25;
            Vector2 buttonCenterPosition = new Vector2(buttonCenterX, buttonCenterY);
            _gameButtons = new Dictionary<string, Button>()
            {
                ["play"] = new Button(
                    "button-start",
                    Content,
                    new Vector2(buttonCenterPosition.X, buttonCenterPosition.Y),
                    GameState.TitleScreen,
                    2
                ),
                ["help"] = new Button(
                    "button-help",
                    Content,
                    new Vector2(buttonCenterPosition.X, buttonCenterPosition.Y + 120),
                    GameState.TitleScreen,
                    2
                ),
                ["back"] = new Button(
                    "button-back",
                    Content,
                    new Vector2(64, _graphics.PreferredBackBufferHeight - 32 * 2 - 20), // Positionera knappen längst ner till vänster på skärmen
                    GameState.HelpScreen,
                    1
                ),
            };
            //...och ställ in djupet samt andra variabler
            _depth = 0;
            _score = 0;
            _scoreUntilNextDepth = 10;
            _maxDepth = _graphics.PreferredBackBufferHeight;
            _timeSinceLastAnimation = 0;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Window.Title = "Fishy av 20alse";
            // Ladda in typsnitt
            _mainFont = Content.Load<SpriteFont>("mainFont");
            _mainFontSmall = Content.Load<SpriteFont>("mainFontSmall");
            _titleFont = Content.Load<SpriteFont>("titleFont");
            // Fiskaren är animerad, så det finns fler bilder på honom. Därför laddar vi in allihopa.
            _fishermanImages = new List<Texture2D>();
            for (int i = 0; i < _fisherManAnimationsCount; i++)
            {
                _fishermanImages.Add(Content.Load<Texture2D>($"fisherman-{i}"));
                Debug.WriteLine($"Lade till fiskarbild {i + 1}.");
            }
            ;
            // Använd den första bilden i animationen
            _fishermanImage = _fishermanImages[0];
            _fisherman.Asset = _fishermanImage;

            // Ladda in instruktionsbilder
            _instructionImage = Content.Load<Texture2D>("idle-screen-instruction");
            _mainInstructionImage = Content.Load<Texture2D>("fishy-instructions-1");
            _mainInstructionImage2 = Content.Load<Texture2D>("fishy-instructions-2");
            // Ladda in logotyp
            _gameLogo = Content.Load<Texture2D>("fishy-logo");

            // Ladda in fisksiluett
            _fishSilhouette = Content.Load<Texture2D>("fish_silhouette");

            // Ladda in ikoner
            _hourGlassImage = Content.Load<Texture2D>("hourglass");

            // Ladda in alla bilder för fiskar
            foreach (FishData fish in _gameFishies)
            {
                _fishImages.Add(fish.Name, Content.Load<Texture2D>(fish.AssociatedAssetName));
            }
            // Ladda in alla bilder för powerups
            foreach (BackgroundImageData backgroundImage in _gameFishingBackgroundItems)
            {
                _fishingBackgroundItems[backgroundImage.Name] = Content.Load<Texture2D>(
                    backgroundImage.AssociatedAssetName
                );
            }
            // Ladda in alla bilder för powerups
            foreach (PowerUpData powerUp in _gamePowerUps)
            {
                _powerUpImages[powerUp.Name] = Content.Load<Texture2D>(powerUp.AssociatedAssetName);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            // Hantera och möjliggör att stoppa spelet med hjälp av escape
            if (
                GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape)
            )
            {
                Exit();
            }

            MouseState _mouseState = Mouse.GetState();
            // Uppdatera alla knappars bilder baserade på om muspekaren är över de eller inte (detta händer endast i minnet, relevanta knappar ritas ut under Draw)
            Dictionary<string, Button> _tempGameButtons = new Dictionary<string, Button>(
                _gameButtons
            ); // Skapa temporär dictionary med knappar
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

            // Nu börjar den specialiserade utritningskoden, dvs. där saker som bara ska vara på en skärm ritas ut.
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
            else if (_state == GameState.HelpScreen)
            {
                // Implementera en bakåtknapp på "hjälpskärmen"
                if (_gameButtons["back"].IsClicked(_mouseState, _previousMouseState))
                {
                    _state = GameState.TitleScreen;
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
                        if (_fishingRod.Position.X >= 0)
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
                        if (_fishingRodCooldown == 0)
                        { // Värdet är 0 när cooldownen inte har startat än. Det återställs av koden när kollisionen inleds
                            _fishingRodCooldown = gameTime.TotalGameTime.TotalMilliseconds;
                            _fishingRod.Position = new Vector2(0, 0); // Flytta tillbaka fiskespöet till startpositionen
                        }
                        if (
                            _fishingRod.Cooldown * _cooldownMultiplier
                            <= (gameTime.TotalGameTime.TotalMilliseconds - _fishingRodCooldown)
                        )
                        {
                            Debug.WriteLine("Återställer fiskespö...");
                            _fishingRod.MarkAsNotCollidedWith();
                            _fishingRod.Position = new Vector2(0, 0); // Flytta tillbaka fiskespöet till startpositonen
                            _depth = 0; // Återställ aktuellt djup
                            _fishingRodCooldown = 0;
                            if (_fishingRod.CollidedItem.IsFish)
                            { // Öka poängen om det är en fisk som nyss har fiskats upp
                                _score += (int)Math.Round(_scoreMultiplier);
                            }
                        }
                        else
                        {
                            Debug.WriteLine(
                                $"Fiskespöet är fortfarande på cooldown... {gameTime.TotalGameTime.TotalMilliseconds - _fishingRodCooldown}/{_fishingRod.Cooldown} millisekunder."
                            );
                        }
                    }
                }
                // Hantera powerups. Se till att de som ska vara aktiverade är det.
                bool resetPowerUps = false;
                foreach (PowerUp powerUp in _currentActivatedPowerUps)
                {
                    Debug.WriteLine(
                        $"Kriterier: {powerUp.IsActivated}, {powerUp.HasBeenCollidedWith}"
                    );
                    if (!powerUp.IsActivated && powerUp.HasBeenCollidedWith) // Om powerupen har kolliderats med av fiskespöet men inte aktiverats så vill vi ju aktivera den. Det gör vi här!
                    {
                        Debug.WriteLine($"Aktiverar en powerup av typen {powerUp.Data.Type}...");
                        powerUp.activatePowerUp(gameTime.TotalGameTime.TotalSeconds);
                        if (powerUp.Data.Type == PowerUpData.PowerUpTypes.SleepPill)
                        {
                            powerUp.savePreviousValue(_speedMultiplier);
                        }
                        else if (powerUp.Data.Type == PowerUpData.PowerUpTypes.Healer)
                        {
                            powerUp.savePreviousValue(_cooldownMultiplier);
                        }
                        if (powerUp.Data.Type == PowerUpData.PowerUpTypes.FishMultiplier)
                        {
                            powerUp.savePreviousValue(_fishMultiplier);
                        }
                        if (powerUp.Data.Type == PowerUpData.PowerUpTypes.ScoreMultiplier)
                        {
                            powerUp.savePreviousValue(_scoreMultiplier);
                        }
                    }
                    if (powerUp.CheckPowerUpStillActive(gameTime.TotalGameTime.TotalSeconds)) // Kontrollera om powerupen fortfarande är aktiverad.
                    {
                        // Isåfall, se till att spelet uppdateras så att man drar fördel av powerupsen
                        // Information: Se klassen "PowerUpData" för mer information om powerups :))
                        if (powerUp.Data.Type == PowerUpData.PowerUpTypes.SleepPill)
                        {
                            _speedMultiplier = powerUp.PreviousValue + powerUp.Data.Multiplier;
                        }
                        else if (powerUp.Data.Type == PowerUpData.PowerUpTypes.Healer)
                        {
                            _cooldownMultiplier = powerUp.PreviousValue + powerUp.Data.Multiplier;
                        }
                        else if (powerUp.Data.Type == PowerUpData.PowerUpTypes.FishMultiplier)
                        {
                            _fishMultiplier = powerUp.PreviousValue + powerUp.Data.Multiplier;
                        }
                        else if (powerUp.Data.Type == PowerUpData.PowerUpTypes.ScoreMultiplier)
                        {
                            _scoreMultiplier = powerUp.PreviousValue + powerUp.Data.Multiplier;
                        }
                    }
                    else
                    {
                        // Återställ spelets status när powerups blir inaktiva
                        // Information: Se klassen "PowerUpData" för mer information om powerups :))
                        Debug.WriteLine("Återställer powerup...");
                        resetPowerUps = true;
                        if (powerUp.Data.Type == PowerUpData.PowerUpTypes.SleepPill)
                        {
                            _speedMultiplier = powerUp.PreviousValue;
                        }
                        else if (powerUp.Data.Type == PowerUpData.PowerUpTypes.Healer)
                        {
                            _cooldownMultiplier = powerUp.PreviousValue;
                        }
                        else if (powerUp.Data.Type == PowerUpData.PowerUpTypes.FishMultiplier)
                        {
                            _fishMultiplier = powerUp.PreviousValue;
                        }
                        else if (powerUp.Data.Type == PowerUpData.PowerUpTypes.ScoreMultiplier)
                        {
                            _scoreMultiplier = powerUp.PreviousValue;
                        }
                    }
                }
                if (resetPowerUps) // Rensa poweruplistor om så ska göras
                {
                    _currentActivatedPowerUps.Clear();
                    _currentPowerUps.Clear();
                }
                // Hantera upplåsning av ny nivå
                if (_score >= _scoreUntilNextDepth)
                {
                    Debug.WriteLine("Ny nivå ska låsas upp!");
                    _maxDepth = _maxDepth * 2; // Öka maxdjupet med 2 ggr
                    _scoreUntilNextDepth = _scoreUntilNextDepth * 2; // Öka gräns för nästa level med 2 ggr.
                    _currentLevel += 1;
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
                    drawScaled(button.ActiveAsset, button.Position, button.Scale);
                }
            }
            if (_state == GameState.TitleScreen)
            {
                // Kod för titelskärmen
                // Rita ut de objekt som ska vara på titelskärmen
                drawScaled(
                    _gameLogo,
                    new Vector2(_graphics.PreferredBackBufferWidth / 2 - _gameLogo.Width * 2, 0),
                    4
                );
            }
            else if (_state == GameState.HelpScreen)
            {
                // Rita ut det som ska vara på hjälpskärmen
                Vector2 instructionScreenHeaderTextSize = _titleFont.MeasureString("INSTRUKTIONER");
                _spriteBatch.DrawString( // Rita ut en sidrubrik till vänster
                    _titleFont,
                    "INSTRUKTIONER",
                    new Vector2(
                        instructionScreenHeaderTextSize.X / 2,
                        instructionScreenHeaderTextSize.Y
                    ),
                    Color.White
                );
                // Rita ut de två statiska instruktionsbilderna
                Vector2 mainInstructionImage1Pos = new Vector2(
                    (_graphics.PreferredBackBufferHeight - _mainInstructionImage.Height) / 2,
                    instructionScreenHeaderTextSize.Y + 30
                );
                _spriteBatch.Draw(_mainInstructionImage, mainInstructionImage1Pos, Color.White);
                _spriteBatch.Draw(
                    _mainInstructionImage2,
                    new Vector2(
                        mainInstructionImage1Pos.X + _mainInstructionImage.Width + 10,
                        mainInstructionImage1Pos.Y
                    ),
                    Color.White
                );
            }
            else if (_state == GameState.IdleScreen)
            {
                // Visa bilden på fiskaren
                drawScaled(_fishermanImage, new Vector2(0, 0), 3);
                // Lägg till en instruerande text
                drawScaled(
                    _instructionImage,
                    new Vector2(
                        (_graphics.PreferredBackBufferWidth - _instructionImage.Width) / 2,
                        ((_graphics.PreferredBackBufferHeight - _instructionImage.Height) / 2) - 45
                    ),
                    2
                );
            }
            else if (_state == GameState.IdleScreenAnimating)
            {
                // Ladda in fiskaren
                // Hämta nästa bild i animationen om vi just nu animerar
                int _fishermanImageIndex = _fishermanImages.IndexOf(_fishermanImage);
                if (gameTime.TotalGameTime.TotalMilliseconds - _timeSinceLastAnimation > 1000) // Uppdatera animation varje sekund.
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
                // Skapa fiskar. Målet är att ha 5 fiskar (normalt) som visas på skärmen samtidigt.
                // Kontrollera vilka fiskar som är möjliga för det djupet vi har
                double depthInMetres = getDepthInMetres(); // Konvertera pixlar till meter
                List<FishData> _possibleFishes = _gameFishies
                    .Where(fish => fish.IsAvailableAt(depthInMetres))
                    .ToList();
                Debug.WriteLine($"{_possibleFishes.Count} fiskar tillgängliga");
                int fishesToCreate =
                    (int)Math.Round(_fishCountTarget * _fishMultiplier) - _currentFishies.Count; // Hur många fiskar som ska skapas
                for (int i = 0; i < fishesToCreate; i++)
                {
                    Debug.WriteLine($"Skapar en ny fisk... ({i + 1}/{fishesToCreate})");
                    FishData newFishData = _possibleFishes[_random.Next(0, _possibleFishes.Count)];
                    Debug.WriteLine($"Den nya fisken är en {newFishData.Name}.");
                    Fish newFish = new Fish(
                        generateRandomStartPosition(), // Börja på en slumpvis höjd
                        newFishData,
                        _fishImages[newFishData.Name], // Hämta föremålets bild
                        _depth
                    );
                    _currentFishies.Add(newFish);
                }
                ;
                // Skapa också föremål i bakgrunden för att skapa en levande bakgrund
                int backgroundImagesToCreate = 15 - _currentFishingBackgroundItems.Count;
                List<BackgroundImageData> availableBackgroundsImages = _gameFishingBackgroundItems
                    .Where(backgroundImage => backgroundImage.IsAvailableAt(depthInMetres))
                    .ToList();
                for (int i = 0; i < backgroundImagesToCreate; i++)
                {
                    BackgroundImageData randomizedBackgroundImage = availableBackgroundsImages[
                        _random.Next(0, availableBackgroundsImages.Count)
                    ];
                    _currentFishingBackgroundItems.Add(
                        new BackgroundImage(
                            new Vector2(
                                _graphics.PreferredBackBufferWidth
                                    - _random.Next(0, _graphics.PreferredBackBufferWidth),
                                _random.Next(_graphics.PreferredBackBufferHeight)
                            ),
                            _fishingBackgroundItems[randomizedBackgroundImage.Name], // Hämta föremålets bild
                            randomizedBackgroundImage,
                            _depth
                        )
                    );
                    ;
                }
                //..gör detsamma med powerups (alltså skapa powerups)
                if (
                    _currentActivatedPowerUps.Count == 0
                    && _currentPowerUps.Count == 0
                    && _random.Next(1, 100) == 1
                    && gameTime.TotalGameTime.TotalSeconds - _lastPowerUpSpawned >= 5
                ) // En powerup kan spawnas med chansen 1% högst var femte sekund
                {
                    List<PowerUpData> possiblePowerUps = _gamePowerUps
                        .Where(powerup => powerup.IsAvailableAt(depthInMetres))
                        .ToList();
                    PowerUpData randomPowerUp = possiblePowerUps[
                        _random.Next(0, possiblePowerUps.Count)
                    ];
                    Debug.WriteLine("Spawnar en powerup...");
                    _currentPowerUps.Add( // Skapa en ny powerup och lägg till den i listan
                        new PowerUp(
                            generateRandomStartPosition(),
                            randomPowerUp,
                            _powerUpImages[randomPowerUp.Name],
                            _depth
                        )
                    );
                    _lastPowerUpSpawned = (int)gameTime.TotalGameTime.TotalSeconds;
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
                        _depth,
                        _fishingRod.Speed
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
                    fish.Speed = fish.Data.DefaultSpeed * (float)_speedMultiplier; // Uppdatera hastighet baseat på aktiv multiplier
                    fish.CheckAndHandleCollisionWithFishingRod(
                        _fishingRod,
                        _depth,
                        _graphics,
                        Keyboard.GetState()
                    );
                    /*Avkommentera för plats-debugging:
                    (jag lämnade detta i inlämningskoden då det kan vara hjälpsamt om man vill implementera nya saker i framtiden)
                    Rectangle fishRectangle = fish.getAssociatedRectangle();
                    Rectangle fishRectangle2 = new Rectangle(
                        (int)fish.Position.X,
                        (int)fish.Position.X,
                        fish.AssociatedAsset.Width + 5,
                        fish.AssociatedAsset.Height + 5
                    );

                    Texture2D rectangleTexture = new Texture2D(
                        GraphicsDevice,
                        fishRectangle2.Width,
                        fishRectangle2.Height
                    );
                    _spriteBatch.Draw(
                        Content.Load<Texture2D>("placeholder-150"),
                        fishRectangle2,
                        Color.Black
                    ); */

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

                List<PowerUp> _temp_currentPowerUps = new List<PowerUp>(_currentPowerUps);
                foreach (PowerUp powerUp in _currentPowerUps)
                {
                    // Hantera kollision med powerupen
                    powerUp.CheckAndHandleCollisionWithFishingRod(
                        _fishingRod,
                        _depth,
                        _graphics,
                        Keyboard.GetState()
                    );
                    if (powerUp.HasBeenCollidedWith && _currentActivatedPowerUps.Count == 0)
                    {
                        Debug.WriteLine("Kollision med powerup skedde nyss!");
                        _currentActivatedPowerUps.Add(powerUp);
                    }
                    if (powerUp.Position.X == -1) // Negativa koordinater - powerupen ska gömmas från skärmen
                    {
                        Debug.WriteLine("Tar bort en powerup från skärmen...");
                        _temp_currentPowerUps.Remove(powerUp);
                    }
                    else
                    {
                        _spriteBatch.Draw(powerUp.AssociatedAsset, powerUp.Position, Color.White);
                    }
                }
                _currentPowerUps = _temp_currentPowerUps; // Uppdatera lista från temporär lista

                // Rita ut kroken/metspöet/fiskespöet
                _spriteBatch.Draw(
                    _fishingRod.AssociatedAsset,
                    _fishingRod.getNextPos(0, 0, 0, 0), // (0,0,0,0) eftersom variablerna overrideas och inte gör någonting, se funktionen
                    Color.White
                );

                // Om fiskespöet är på "cooldown", rita ut en bild som indikerar det
                if (_fishingRod.HasBeenCollidedWith)
                {
                    _spriteBatch.Draw(_hourGlassImage, new Vector2(0, 0), Color.White);
                }
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

                // Skriv ut gränsen till nästa djup "nivå".
                string pointText = $"Nivågräns: {_scoreUntilNextDepth} poäng";
                _spriteBatch.DrawString(
                    _mainFont,
                    pointText,
                    new Vector2(
                        _graphics.PreferredBackBufferWidth - _mainFont.MeasureString(pointText).X,
                        75
                    ),
                    Color.White
                );

                // Om fiskespöet är nära maxdjupet, rita ut en text
                Vector2 lockedStringTextSize = _mainFont.MeasureString("---LÅST--");
                Debug.WriteLine(
                    $"Debug: gränsvärde för låst text: {_graphics.PreferredBackBufferHeight - _depth}"
                );
                if ((_maxDepth - _depth) <= lockedStringTextSize.Y)
                { //(_depth-variabeln är i antal pixlar)
                    _spriteBatch.DrawString(
                        _mainFont,
                        $"---LÅST---",
                        new Vector2(
                            (_graphics.PreferredBackBufferWidth - lockedStringTextSize.X) / 2,
                            _graphics.PreferredBackBufferHeight - lockedStringTextSize.Y
                        ),
                        Color.White
                    );
                }

                // Rita ut en lista med de fiskar som man har låst upp.
                float yOffset = 100;
                int xOffset = 100; // Denna är konstant
                Vector2 unlockedFishesTextSize = _mainFont.MeasureString("Upplåsta fiskar:");
                _spriteBatch.DrawString(
                    _mainFontSmall,
                    "Upplåsta fiskar:",
                    new Vector2(
                        _graphics.PreferredBackBufferWidth - unlockedFishesTextSize.X,
                        yOffset
                    ),
                    Color.White
                );
                yOffset += unlockedFishesTextSize.Y + 5;
                foreach (FishData fish in _gameFishies)
                {
                    Texture2D fishAsset; // Bilden som ska ritas ut.
                    string fishText;
                    if (fish.IsAvailableAt(getDepthInMetres(_maxDepth)))
                    { // Tillgänglig - hämta bilden som är knuten till fisken
                        fishAsset = _fishImages[fish.Name];
                        fishText = fish.Name;
                    }
                    else
                    { // Inte tillgänglig
                        fishAsset = _fishSilhouette;
                        fishText = "???";
                    }
                    Vector2 fishTextSize = _mainFont.MeasureString(fishText);
                    Vector2 fishAssetPosition = new Vector2(
                        _graphics.PreferredBackBufferWidth - fishAsset.Width / 2 - xOffset,
                        yOffset
                    ); // Positionen för bilden på fisken/fisk-siluetten
                    // Rita ut siluett
                    drawScaled(fishAsset, fishAssetPosition, 0.5f);
                    // Rita ut fiskens namn
                    _spriteBatch.DrawString(
                        _mainFontSmall,
                        fishText,
                        new Vector2(
                            _graphics.PreferredBackBufferWidth - xOffset,
                            yOffset + fishTextSize.Y / 2
                        ),
                        Color.White
                    );
                    yOffset += fishTextSize.Y + 5;
                }
                yOffset += 10;
                // Rita ut aktiva powerups
                foreach (PowerUp activePowerUp in _currentActivatedPowerUps)
                {
                    string powerUpActiveUntilText = activePowerUp.GetActiveUntilText(
                        gameTime.TotalGameTime.TotalSeconds
                    ); // Hämta en text som skriver ut hur länge powerupen är aktiv
                    Vector2 powerUpActiveUntilTextSize = _mainFont.MeasureString(
                        powerUpActiveUntilText
                    ); // Hämta storlek på texten
                    _spriteBatch.DrawString(
                        _mainFont,
                        powerUpActiveUntilText,
                        new Vector2(
                            _graphics.PreferredBackBufferWidth - powerUpActiveUntilTextSize.X - 10,
                            yOffset + activePowerUp.AssociatedAsset.Width / 4
                        ),
                        Color.White
                    ); // Skriv ut tid som det är kvar på powerupen
                    drawScaled(
                        activePowerUp.AssociatedAsset,
                        new Vector2(
                            _graphics.PreferredBackBufferWidth
                                - activePowerUp.AssociatedAsset.Width / 2
                                - powerUpActiveUntilTextSize.X
                                - 10,
                            yOffset
                        ),
                        0.5f
                    );
                    yOffset += powerUpActiveUntilTextSize.Y + 5;
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Eftersom _depth-variabeln är i pixlar så finns det en funktion för att konvertera den till meter i havet som är mer användarvänligt.
        /// </summary>
        /// <param name="depth">Antingen null (om djupet ska hämtas från koden) eller det djup i pixel som ska konverteras.</param>
        /// <returns></returns>
        public double getDepthInMetres(float? depth = null)
        {
            if (depth == null)
            {
                depth = _depth;
            }
            return Math.Round((float)depth / (_graphics.PreferredBackBufferHeight / 8), 1); // 1 m är 1/8 av skärmen
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

        /// <summary>
        /// I spelet ska det skapas ett flertal objekt som ska börja strax under fiskespöet på varje nivå.
        /// För att skapa dessa objekt så skapar vi en funktion som returnerar denna slumpvis position
        /// </summary>
        /// <returns>En slumpvis startposition som en Vector2.</returns>
        public Vector2 generateRandomStartPosition()
        {
            Debug.WriteLine(
                $"Skapar startposition för {_depth}/{_maxDepth} (fiskespöets höjd: {_fishingRod.AssociatedAsset.Height})"
            );

            // Om fiskespöet är nere på maxdjupet så vill vi inte att de föremål som spawnas hamnar under dess krok, dit man inte kan nå.
            int newYPos;
            if (_maxDepth - _depth < _fishingRod.AssociatedAsset.Height) // (djup mäts i pixlar)
            {
                newYPos = _random.Next(0, _fishingRod.AssociatedAsset.Height); // Se till att de nya föremålen hamnar under fiskespöets krok
            }
            else
            {
                newYPos =
                    _graphics.PreferredBackBufferHeight
                    - _random.Next(
                        0,
                        _graphics.PreferredBackBufferHeight
                            - (int)_fishingRod.AssociatedAsset.Height
                            - (int)_fishingRod.Position.Y
                    ); // Se till att de nya föremålen hamnar under fiskespöets krok
            }
            return new Vector2(
                _graphics.PreferredBackBufferWidth
                    - _random.Next(0, _graphics.PreferredBackBufferWidth / 8),
                newYPos
            );
        }
    }
}
