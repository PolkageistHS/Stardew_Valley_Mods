﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PyTK.CustomElementHandler;
using Revitalize.Framework.Graphics;
using Revitalize.Framework.Graphics.Animations;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revitalize.Framework.Objects
{
    /// <summary>
    /// Change draw functions.
    /// </summary>
    public class CustomObject : PySObject
    {

        public string id;
        public BasicItemInformation info;


        public AnimationManager animationManager
        {
            get
            {
                return info.animationManager;
            }
        }


        public Texture2D displayTexture
        {
            get
            {
                return animationManager.getTexture();
            }
        }


        public CustomObject()
        {
            
        }

        public CustomObject(BasicItemInformation info):base(info,Vector2.Zero)
        {
            this.info = info;
            this.initializeBasics();
        }

        public CustomObject(BasicItemInformation info,Vector2 TileLocation) : base(info, TileLocation)
        {
            this.info = info;
            this.initializeBasics();
        }

        public virtual void initializeBasics()
        {
            this.name = info.name;
            this.displayName = getDisplayNameFromStringsFile(this.id);
            this.Edibility = info.edibility;
            this.Category = -9; //For crafting.
            this.displayName = info.name;
            this.setOutdoors.Value = true;
            this.setIndoors.Value = true;
            this.isLamp.Value = false;
            this.fragility.Value = 0;

            this.updateDrawPosition();

            this.bigCraftable.Value = false;
            
        }

        public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        {
            var mState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            var keyboardState = Game1.GetKeyboardState();

            if (mState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))==false)
            {
                Revitalize.ModCore.log("Right clicked!");
                return rightClicked(who);
            }
            else if(mState.RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed && (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift)))
            {
                return shiftRightClicked(who);
            }

            if (justCheckingForActivity)
            {
                return true;
            }
            Revitalize.ModCore.log("Left clicked!");
            return this.clicked(who);
        }

        public override ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            BasicItemInformation data =(BasicItemInformation) CustomObjectData.collection[additionalSaveData["id"]];
            return new CustomObject((BasicItemInformation) CustomObjectData.collection[additionalSaveData["id"]], (replacement as Chest).TileLocation);
        }


        public virtual bool rightClicked(Farmer who)
        {
            // Game1.showRedMessage("YOOO");
            //do some stuff when the right button is down
            // rotate();
            if (this.heldObject.Value != null)
            {
                //  Game1.player.addItemByMenuIfNecessary(this.heldObject);
                // this.heldObject = null;
            }
            else
            {
                //   this.heldObject = Game1.player.ActiveObject;
                //  Game1.player.removeItemFromInventory(heldObject);
            }
            return true;
        }

        public virtual bool shiftRightClicked(Farmer who)
        {
            Revitalize.ModCore.log("Shift right clicked!");
            return true;
        }

        public override bool clicked(Farmer who)
        {
            Revitalize.ModCore.log("Clicky click!");

            return removeAndAddToPlayersInventory();
            //return base.clicked(who);
        }

        public override bool performToolAction(Tool t, GameLocation location)
        {

            if(t.GetType()== typeof(StardewValley.Tools.Axe) || t.GetType()== typeof(StardewValley.Tools.Pickaxe))
            {
                Game1.createItemDebris(this, Game1.player.getStandingPosition(), Game1.player.getDirection());
                Game1.player.currentLocation.removeObject(this.TileLocation, false);
                return false;
            }

            return false;
            //return base.performToolAction(t, location);
        }

        public virtual bool removeAndAddToPlayersInventory()
        {
            if (Game1.player.isInventoryFull())
            {
                Game1.showRedMessage("Inventory full.");
                return false;
            }
            Game1.player.currentLocation.removeObject(this.TileLocation, false);
            Game1.player.addItemToInventory(this);
            return true;
        }

        public override Color getCategoryColor()
        {
            return info.categoryColor;
            //return data.categoryColor;
        }

        public override string getCategoryName()
        {
            return info.categoryName;
        }

        public override string getDescription()
        {
            string text = info.description;
            SpriteFont smallFont = Game1.smallFont;
            int width = Game1.tileSize * 4 + Game1.tileSize / 4;
            return Game1.parseText(text, smallFont, width);
        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            this.updateDrawPosition();
            return base.placementAction(location, x, y, who);
        }

        public virtual void updateDrawPosition()
        {
            this.info.drawPosition = new Vector2((float)this.boundingBox.X, (float)(this.boundingBox.Y - (this.animationManager.currentAnimation.sourceRectangle.Height * Game1.pixelZoom - this.boundingBox.Height)));
        }

        public override Item getOne()
        {
            return new CustomObject((BasicItemInformation)this.data);
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            if (x == -1)
            {
                spriteBatch.Draw(this.info.animationManager.getTexture(), Game1.GlobalToLocal(Game1.viewport, this.info.drawPosition), new Rectangle?(this.animationManager.currentAnimation.sourceRectangle), Color.White * alpha, 0f, Vector2.Zero, (float)Game1.pixelZoom, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            }
            else
            {
                //The actual planter box being drawn.
                if (animationManager == null)
                {
                    if (this.animationManager.getExtendedTexture() == null)
                    {
                        ModCore.ModMonitor.Log("Tex Extended is null???");

                    }

                    spriteBatch.Draw(this.displayTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * Game1.tileSize), y * Game1.tileSize)), new Rectangle?(this.animationManager.currentAnimation.sourceRectangle), Color.White * alpha, 0f, Vector2.Zero, (float)Game1.pixelZoom, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
                    // Log.AsyncG("ANIMATION IS NULL?!?!?!?!");
                }

                else
                {
                    //Log.AsyncC("Animation Manager is working!");
                    this.animationManager.draw(spriteBatch, this.displayTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * Game1.tileSize), y * Game1.tileSize)), new Rectangle?(this.animationManager.currentAnimation.sourceRectangle), Color.White * alpha, 0f, Vector2.Zero, (float)Game1.pixelZoom, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
                    try
                    {
                        this.animationManager.tickAnimation();
                        // Log.AsyncC("Tick animation");
                    }
                    catch (Exception err)
                    {
                        ModCore.ModMonitor.Log(err.ToString());
                    }
                }

                // spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)((double)tileLocation.X * (double)Game1.tileSize + (((double)tileLocation.X * 11.0 + (double)tileLocation.Y * 7.0) % 10.0 - 5.0)) + (float)(Game1.tileSize / 2), (float)((double)tileLocation.Y * (double)Game1.tileSize + (((double)tileLocation.Y * 11.0 + (double)tileLocation.X * 7.0) % 10.0 - 5.0)) + (float)(Game1.tileSize / 2))), new Rectangle?(new Rectangle((int)((double)tileLocation.X * 51.0 + (double)tileLocation.Y * 77.0) % 3 * 16, 128 + this.whichForageCrop * 16, 16, 16)), Color.White, 0.0f, new Vector2(8f, 8f), (float)Game1.pixelZoom, SpriteEffects.None, (float)(((double)tileLocation.Y * (double)Game1.tileSize + (double)(Game1.tileSize / 2) + (((double)tileLocation.Y * 11.0 + (double)tileLocation.X * 7.0) % 10.0 - 5.0)) / 10000.0));



            }
        }

        public override void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1f)
        {

            if (Game1.eventUp && Game1.CurrentEvent.isTileWalkedOn(xNonTile / 64, yNonTile / 64))
                return;
            if ((int)(this.ParentSheetIndex) != 590 && (int)(this.Fragility) != 2)
                spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(xNonTile + 32), (float)(yNonTile + 51 + 4))), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White * alpha, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, layerDepth - 1E-06f);
            SpriteBatch spriteBatch1 = spriteBatch;
            Texture2D objectSpriteSheet = Game1.objectSpriteSheet;
            Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(xNonTile + 32 + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0)), (float)(yNonTile + 32 + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0))));
            Microsoft.Xna.Framework.Rectangle? sourceRectangle = new Microsoft.Xna.Framework.Rectangle?(GameLocation.getSourceRectForObject(this.ParentSheetIndex));
            Color color = Color.White * alpha;
            double num1 = 0.0;
            Vector2 origin = new Vector2(8f, 8f);
            Vector2 scale = this.scale;
            double num2 = (double)this.scale.Y > 1.0 ? (double)this.getScale().Y : 4.0;
            int num3 = (bool)(this.flipped) ? 1 : 0;
            double num4 = (double)layerDepth;

            spriteBatch1.Draw(this.displayTexture, local, this.animationManager.defaultDrawFrame.sourceRectangle, color, (float)num1, origin, (float)num2, (SpriteEffects)num3, (float)num4);

        }

       

        public override void drawAsProp(SpriteBatch b)
        {
            base.drawAsProp(b);
        }

        public override void drawAttachments(SpriteBatch b, int x, int y)
        {
            base.drawAttachments(b, x, y);
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber, Color c, bool drawShadow)
        {
            if (drawStackNumber && this.maximumStackSize() > 1 && ((double)scaleSize > 0.3 && this.Stack != int.MaxValue) && this.Stack > 1)
                Utility.drawTinyDigits(this.Stack, spriteBatch, location + new Vector2((float)(Game1.tileSize - Utility.getWidthOfTinyDigitString(this.Stack, 3f * scaleSize)) + 3f * scaleSize, (float)((double)Game1.tileSize - 18.0 * (double)scaleSize + 2.0)), 3f * scaleSize, 1f, Color.White);
            if (drawStackNumber && this.Quality > 0)
            {
                float num = this.Quality < 4 ? 0.0f : (float)((Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1.0) * 0.0500000007450581);
                spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(12f, (float)(Game1.tileSize - 12) + num), new Microsoft.Xna.Framework.Rectangle?(this.Quality < 4 ? new Microsoft.Xna.Framework.Rectangle(338 + (this.Quality - 1) * 8, 400, 8, 8) : new Microsoft.Xna.Framework.Rectangle(346, 392, 8, 8)), Color.White * transparency, 0.0f, new Vector2(4f, 4f), (float)(3.0 * (double)scaleSize * (1.0 + (double)num)), SpriteEffects.None, layerDepth);
            }
            spriteBatch.Draw(this.displayTexture, location + new Vector2((float)(Game1.tileSize), (float)(Game1.tileSize)), new Rectangle?(this.animationManager.currentAnimation.sourceRectangle), Color.White * transparency, 0f, new Vector2((float)(this.animationManager.currentAnimation.sourceRectangle.Width / 2), (float)(this.animationManager.currentAnimation.sourceRectangle.Height)), scaleSize, SpriteEffects.None, layerDepth);
        }

        public override void drawPlacementBounds(SpriteBatch spriteBatch, GameLocation location)
        {
            base.drawPlacementBounds(spriteBatch, location);
        }

        public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, StardewValley.Farmer f)
        {
            if (f.ActiveObject.bigCraftable.Value)
            {
                spriteBatch.Draw(this.displayTexture, objectPosition, this.animationManager.currentAnimation.sourceRectangle, Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 2) / 10000f));
                return;
            }

            spriteBatch.Draw(this.displayTexture, objectPosition, this.animationManager.currentAnimation.sourceRectangle, Color.White, 0f, Vector2.Zero, (float)Game1.pixelZoom, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 2) / 10000f));
            if (f.ActiveObject != null && f.ActiveObject.Name.Contains("="))
            {
                spriteBatch.Draw(this.displayTexture, objectPosition + new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), this.animationManager.currentAnimation.sourceRectangle, Color.White, 0f, new Vector2((float)(Game1.tileSize / 2), (float)(Game1.tileSize / 2)), (float)Game1.pixelZoom + Math.Abs(Game1.starCropShimmerPause) / 8f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 2) / 10000f));
                if (Math.Abs(Game1.starCropShimmerPause) <= 0.05f && Game1.random.NextDouble() < 0.97)
                {
                    return;
                }
                Game1.starCropShimmerPause += 0.04f;
                if (Game1.starCropShimmerPause >= 0.8f)
                {
                    Game1.starCropShimmerPause = -0.8f;
                }
            }
            //base.drawWhenHeld(spriteBatch, objectPosition, f);
        }


        public string getDisplayNameFromStringsFile(string objectID)
        {
            //Load in a file that has all object names referenced here or something.
            return info.name;
        }

    }
}