using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ExtremelySimpleLogger;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Data;
using MLEM.Data.Content;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using TinyLife;
using TinyLife.Actions;
using TinyLife.Emotions;
using TinyLife.Mods;
using TinyLife.Objects;
using TinyLife.Utilities;
using TinyLife.World;
using TinyLife.Tools;
using Action = TinyLife.Actions.Action;
using MLEM.Misc;
using TinyLife.Skills;

namespace CastleKit;

public class CastleKit : Mod
{

    // the logger that we can use to log info about this mod
    public static Logger Logger { get; private set; }

    public override string Name => "Medieval Living Kit";
    public override string Description => "Furnish your Medieval Homes! - v1.0";
    public override string IssueTrackerUrl => "https://x.com/RedGindew";
    public override string TestedVersionRange => "[0.45.0]";
    private Dictionary<Point, TextureRegion> uiTextures, Floor, FenceTex;
    private Dictionary<Point, TextureRegion> cuffShirt, nobleHat, maidenDress;
    private Dictionary<Point, TextureRegion> windows, wallpaper;
    public override TextureRegion Icon => this.uiTextures[new Point(0, 0)];


    public override void Initialize(Logger logger, RawContentManager content, RuntimeTexturePacker texturePacker, ModInfo info)
    {
        CastleKit.Logger = logger;
        texturePacker.Add(new UniformTextureAtlas(content.Load<Texture2D>("UITex"), 8, 8), r => this.uiTextures = r, 1, true);
        texturePacker.Add(new UniformTextureAtlas(content.Load<Texture2D>("Tiles"), 6, 4), r => this.Floor = r, 1, true);
        texturePacker.Add(new UniformTextureAtlas(content.Load<Texture2D>("Fences"), 11, 4), r => this.FenceTex = r, 1, true);
        texturePacker.Add(new UniformTextureAtlas(content.Load<Texture2D>("Windows"), 16, 5), r => this.windows = r, 1, true);
        WallMode.ApplyMasks(content.Load<Texture2D>("Wallpapers"), 4, 5, texturePacker, r => this.wallpaper = r);
        //texturePacker.Add(new UniformTextureAtlas(content.Load<Texture2D>("Openings"), 16, 5), r => this.openings = r, 1, true);

        texturePacker.Add(new UniformTextureAtlas(content.Load<Texture2D>("CuffShirt"), 8, 11), r => this.cuffShirt = r, 1, true);

        texturePacker.Add(new UniformTextureAtlas(content.Load<Texture2D>("MaidenDress"), 4, 6), r => this.maidenDress = r, 1, true);

        texturePacker.Add(new UniformTextureAtlas(content.Load<Texture2D>("NobleHat"), 4, 5), r => this.nobleHat = r, 1, true);
    }

    public override void AddGameContent(GameImpl game, ModInfo info)
    {
        FurnitureType.Register(new FurnitureType.TypeSettings("CastleKit.OldBookshelf", new Point(1, 1), ObjectCategory.Bookshelf, 50)
        {
            Icon = this.Icon,
            Tab = (FurnitureTool.Tab.Office),
            Colors = new ColorSettings(ColorScheme.SimpleWood, ColorScheme.MutedPastels) { Defaults = new int[] { 6, 4 }, PreviewName = "CastleKit.OldBookshelf" },
            DefaultRotation = MLEM.Maths.Direction2.Right,
        });
        FurnitureType.Register(new FurnitureType.TypeSettings("CastleKit.OldBookshelfLadder", new Point(1, 1), ObjectCategory.Bookshelf, 50)
        {
            Icon = this.Icon,
            Tab = (FurnitureTool.Tab.Office),
            Colors = new ColorSettings(ColorScheme.SimpleWood, ColorScheme.MutedPastels) { Map = new int[] { 0, 1, 0 }, Defaults = new int[] { 6, 4 }, PreviewName = "CastleKit.OldBookshelfLadder" },
            DefaultRotation = MLEM.Maths.Direction2.Right,
        });
        FurnitureType.Register(new FurnitureType.TypeSettings("CastleKit.MedievalRug", new Point(3, 4), ObjectCategory.GroundItem, 50)
        {
            Icon = this.Icon,
            Tab = (FurnitureTool.Tab.Decoration),
            Colors = new ColorSettings(ColorScheme.White) { Defaults = new int[] { 0 } },
            DefaultRotation = MLEM.Maths.Direction2.Right,
        });
        FurnitureType.Register(new FurnitureType.TypeSettings("CastleKit.WarTable", new Point(2, 2), ObjectCategory.ArtPiece, 50)
        {
            Icon = this.Icon,
            Tab = (FurnitureTool.Tab.Decoration),
            Colors = new ColorSettings(ColorScheme.WarmDarkMutedPastels, ColorScheme.White) { Defaults = new int[] { 0, 0 }, PreviewName = "CastleKit.WarTable" },
            DefaultRotation = MLEM.Maths.Direction2.Up,
        });
        FurnitureType.Register(new FurnitureType.TypeSettings("CastleKit.MedievalTable", new Point(1, 2), ObjectCategory.Table, 150)
        {
            Icon = this.Icon,
            Tab = (FurnitureTool.Tab.DiningRoom),
            Colors = new ColorSettings(ColorScheme.SimpleWood) { Defaults = new int[] { 1 }, PreviewName = "CastleKit.MedievalTable" },
            ObjectSpots = ObjectSpot.TableSpots(new Point(1, 2)).ToArray()
        });
        FurnitureType.Register(new FurnitureType.TypeSettings("CastleKit.MedievalStool", new Point(1, 1), ObjectCategory.Chair, 160)
        {
            Icon = this.Icon,
            Tab = (FurnitureTool.Tab.DiningRoom),
            Colors = new ColorSettings(ColorScheme.SimpleWood) { Defaults = new int[] { 1 }, PreviewName = "CastleKit.MedievalStool" },
            ObjectSpots = { },
            ActionSpots = new[] {new ActionSpot(Vector2.Zero, 0.1F, MLEM.Maths.Direction2Helper.Adjacent) {
                    DrawLayer = f => 0
                    }
                },
        });
        FurnitureType.Register(new FurnitureType.TypeSettings("CastleKit.CatherinePainting", new Point(1, 1), ObjectCategory.WallHanging | ObjectCategory.NonColliding, 120)
        {
            Icon = this.Icon,
            Tab = (FurnitureTool.Tab.Decoration),
            Colors = new ColorSettings(new ColorScheme[] { ColorScheme.SheetMetal, ColorScheme.White }) { Defaults = new int[] { 0, 0 }, PreviewName = "CastleKit.CatherinePainting" },
            DefaultRotation = MLEM.Maths.Direction2.Right
        });
        FurnitureType.Register(new FurnitureType.TypeSettings("CastleKit.HangingBanner", new Point(1, 1), ObjectCategory.WallHanging | ObjectCategory.NonColliding, 120)
        {
            Icon = this.Icon,
            Tab = (FurnitureTool.Tab.Decoration),
            Colors = new ColorSettings(new ColorScheme[] { ColorScheme.White, ColorScheme.WarmDarkMutedPastels }) { Defaults = new int[] { 0, 0 }, PreviewName = "CastleKit.HangingBanner" },
            DefaultRotation = MLEM.Maths.Direction2.Right
        });
        FurnitureType.Register(new FurnitureType.TypeSettings("CastleKit.HenryOfTiny", new Point(1, 1), ObjectCategory.WallHanging | ObjectCategory.NonColliding, 120)
        {
            Icon = this.Icon,
            Tab = (FurnitureTool.Tab.Decoration),
            Colors = new ColorSettings(new ColorScheme[] { ColorScheme.SheetMetal, ColorScheme.White }) { Defaults = new int[] { 0, 0 }, PreviewName = "CastleKit.HenryOfTiny" },
            DefaultRotation = MLEM.Maths.Direction2.Right
        });
        FurnitureType.Register(new FurnitureType.TypeSettings("CastleKit.StuartOfTiny", new Point(1, 1), ObjectCategory.WallHanging | ObjectCategory.NonColliding, 120)
        {
            Icon = this.Icon,
            Tab = (FurnitureTool.Tab.Decoration),
            Colors = new ColorSettings(new ColorScheme[] { ColorScheme.SheetMetal, ColorScheme.White }) { Defaults = new int[] { 0, 0 }, PreviewName = "CastleKit.StuartOfTiny" },
            DefaultRotation = MLEM.Maths.Direction2.Right
        });
        FurnitureType.Register(new FurnitureType.TypeSettings("CastleKit.MedievalSideTable", new Point(1, 1), ObjectCategory.Table, 75)
        {
            Icon = this.Icon,
            Tab = (FurnitureTool.Tab.Bedroom),
            Colors = new ColorSettings(ColorScheme.SimpleWood) { Defaults = new int[] { 1 }, PreviewName = "CastleKit.MedievalSideTable" },
            ObjectSpots = ObjectSpot.TableSpots(new Point(33, 5)).ToArray()
        });

        // Fencing
        FenceType.Register(new FenceType("CastleKit.CastleWall", 10, this.Icon, this.FenceTex, new Point(0, 0), ColorScheme.GraysCeramics));

        //Openings
        //WallMode.Register(new WallMode("", new UniformTextureAtlas(new TextureRegion(, 10, 10), 1, 1, 0), new Point(0, 0), null));
        Wallpaper.Register("CastleKit.WallPanel", 15, this.wallpaper, new Point(0, 0), ColorScheme.SimpleWood, this.Icon);

        // Windows
        OpeningType.Register(new OpeningType("CastleKit.NarrowWindow", this.windows, new Point(0, 0), WallMode.NarrowLong, 100, ColorScheme.Bricks, 1, null, this.Icon));
        OpeningType.Register(new OpeningType("CastleKit.FiringHole", this.windows, new Point(2, 0), WallMode.SmallWindow, 80, ColorScheme.Bricks, 1, null, this.Icon));
        OpeningType.Register(new OpeningType("CastleKit.BarredWindow", this.windows, new Point(6, 0), WallMode.LongWindow, 80, ColorScheme.Bricks, 1, null, this.Icon));

        // Flooring
        Tile.Register("CastleKit.GardenTile", 20, this.Floor, new Point(0, 0), new ColorScheme[] { ColorScheme.Plants, ColorScheme.Grays }, 0, true, Tile.Category.Natural, this.Icon);
        Tile.Register("CastleKit.GardenStone", 20, this.Floor, new Point(0, 1), new ColorScheme[] { ColorScheme.Plants, ColorScheme.Grays }, 0, true, Tile.Category.Natural, this.Icon);
        Tile.Register("CastleKit.CastleBrick", 20, this.Floor, new Point(0, 2), new ColorScheme[] { ColorScheme.GraysCeramics }, 0, true, Tile.Category.None, this.Icon);


        // Clothing
        Clothes.Register(new Clothes("CastleKit.MedievalCuffShirt", ClothesLayer.Shirt, this.cuffShirt, new Point(0, 0), 100, ClothesIntention.Everyday, StylePreference.Neutral, new ColorScheme[] { ColorScheme.WarmDarkMutedPastels, ColorScheme.White }) { Icon = this.Icon });
        Clothes.Register(new Clothes("CastleKit.NoblemanHat", ClothesLayer.HeadAccessories, this.nobleHat, new Point(0, 0), 100, ClothesIntention.Everyday, StylePreference.Neutral, ColorScheme.WarmDarkMutedPastels)
        {
            Icon = this.Icon,
            LayersToHide = ClothesLayer.Hair
        }
        );
        Clothes.Register(new Clothes("CastleKit.MaidenDress", ClothesLayer.Pants, this.maidenDress, new Point(0, 0), 100, ClothesIntention.Everyday, StylePreference.Neutral, ColorScheme.WarmDarkMutedPastels)
        {
            Icon = this.Icon,
            LayersToHide = ClothesLayer.Shoes
        }
        );
    }

    public override IEnumerable<string> GetCustomFurnitureTextures(ModInfo info)
    {
        yield return "OldBookshelf";
        yield return "OldBookshelfLadder";
        yield return "MedievalRug";
        yield return "MedievalTable";
        yield return "MedievalSideTable";
        yield return "MedievalStool";
        yield return "CatherinePainting";
        yield return "HenryOfTiny";
        yield return "StuartOfTiny";
        yield return "HangingBanner";
        yield return "WarTable";
    }
}