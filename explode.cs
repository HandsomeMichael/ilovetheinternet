using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using MonoMod.Cil;
using ReLogic.Graphics;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Terraria;
using Terraria.Localization; 
using Terraria.Utilities;
using Terraria.GameContent.Dyes;
using Terraria.GameContent.UI;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace ilovetheinternet
{
	public class Explode
	{
		public int frame;

		public int frameCounter;

		public Vector2 position;

		public float scale;

		public bool active;

		public const int maxFrame = 17;

		public Rectangle getRect() {
			NPC npc = new NPC();
			npc.SetDefaults(NPCID.BlueSlime);
			npc.position = position;
			return npc.getRect();
		}

		public void Update(){
			if (!active){return;}
			frameCounter++;
			if (frameCounter > ExplodeConfig.get.explodefps) {
				frame++;
				frameCounter = 0;
			}
			if (frame == maxFrame){
				active = false;
				return;
			}
		}

		public static void Updateexplode()
		{
			List<Explode> ded = new List<Explode>();
			foreach (Explode explode in ilovetheinternet.explodeList)
			{
				if (explode != null) {
					explode.Update();
					if (!explode.active) {ded.Add(explode);}
				}
			}
			foreach (Explode explode in ded) {
				if (explode != null) {
					ilovetheinternet.explodeList.Remove(explode);
				}
			}
		}
		public static void New(Vector2 pos,float scale = -1f, bool rand = true) 
		{
			Explode explode = new Explode();
			explode.position = pos;
			if (rand) {
				explode.position.X += Main.rand.Next(-10,11);
				explode.position.Y += Main.rand.Next(-10,11);
			}
			float a = ExplodeConfig.get.explodesize;
			if (scale != -1f) {a = scale;}
			explode.scale = a;
			explode.active = true;
			ilovetheinternet.explodeList.Add(explode);
			if (ExplodeConfig.get.explodesound) {
				Mod mod = ModLoader.GetMod("ilovetheinternet");
				Main.PlaySound(mod.GetLegacySoundSlot(SoundType.Custom, "Sounds/Custom/explode"),pos);
			}
		}
		public static void Drawexplode(Explode explode)
		{
			if (explode == null) {
				return;
			}
			if (!explode.active && explode.frame >= maxFrame) {
				return;
			}
			Texture2D tt = ModContent.GetTexture("ilovetheinternet/Explode/explode_"+explode.frame);
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);			
			Main.spriteBatch.Draw(tt, explode.position - Main.screenPosition, null, Color.White*ExplodeConfig.get.explodealpha, 0, tt.Size() * 0.5f, explode.scale, SpriteEffects.None, 0f);
			//
			Main.spriteBatch.End();
			//spriteBatch.End();
			//spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.ZoomMatrix);
		}
		public static void AddExplode() {
			On.Terraria.Main.DoDraw += drawAdd;
			On.Terraria.Main.Update += updateAdd;
		}
		private static void updateAdd(On.Terraria.Main.orig_Update orig,global::Terraria.Main self, GameTime gameTime) {
			if (ExplodeConfig.get.explodefocus) {
				if (Main.hasFocus) {Updateexplode();}
			}
			else {Updateexplode();}
			orig(self,gameTime);
		}
		private static void drawAdd(On.Terraria.Main.orig_DoDraw orig,global::Terraria.Main self, GameTime gameTime) {
			orig(self,gameTime);
			foreach (Explode explode in ilovetheinternet.explodeList)
			{
				if (explode != null) {
					if (explode.active) {
						Drawexplode(explode);
					}
				}
			}
		}
	}
	public class ilovetheinternet : Mod
	{
		public static List<Explode> explodeList = new List<Explode>();
		public override void Load() {
			Explode.AddExplode();
		}
		public override void PreSaveAndQuit() {
			explodeList.Clear();
		}
		public override void Unload() {
			explodeList.Clear();
		}
		class Funny1 : GlobalProjectile
		{
			public override void Kill(Projectile projectile,int timeLeft) {
				Explode.New(projectile.Center);
			}
		}
		class Funny2 : ModPlayer
		{
			public override void OnHitAnything(float x, float y, Entity victim) {
				Explode.New(victim.Center);
			}
		}
		class Funny3 : GlobalNPC
		{
			public override bool CheckDead(NPC npc) {
				Explode.New(npc.Center,npc.scale/2);
				return base.CheckDead(npc);
			}
		}
	}
	[Label("Explode")]
	public class ExplodeConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;
		public static ExplodeConfig get => ModContent.GetInstance<ExplodeConfig>();

		[Header("Explode")]

		[Label("Exploding sound")]
		[Tooltip("Play explode sound when there is an explosion")]
		[DefaultValue(true)]
		public bool explodesound { get; set; }

		[Label("Explode animate only on focus")]
		[Tooltip("Play explode animation only when tmod is focused")]
		[DefaultValue(true)]
		public bool explodefocus { get; set; }

		[Label("Explode Frame rate")]
		[Tooltip("the higher the number, the slower it will be animated \n [default is 5]")]
		[Range(1, 60)]
		[DefaultValue(5)]
		[Slider] 
		public int explodefps { get; set; }

		[Label("Explode Size")]
		[Tooltip("The default size of the explosion \n [default is 0.3]")]
		[Range(0.1f, 1f)]
		[Increment(0.1f)]
		[DefaultValue(0.3f)]
		[Slider]
		public float explodesize { get; set; }

		[Label("Explode Opacity")]
		[Tooltip("The opacity wich the explosion get drawed \n [default is 1]")]
		[Range(0.1f, 1f)]
		[Increment(0.1f)]
		[DefaultValue(1f)]
		[Slider]
		public float explodealpha { get; set; }
	}
	public class explodeItem : ModItem
	{
		public override string Texture => "ilovetheinternet/icon";
		public override void SetStaticDefaults() 
		{
			DisplayName.SetDefault("Create explode");
			Tooltip.SetDefault("will create a Explode upon use");
		}

		public override void SetDefaults() 
		{
			item.useStyle = 1;
            item.width = 14;
            item.height = 14;
            item.rare = 0;
            item.value = 69;
			item.useAnimation = 10;
			item.useTime = 10;
			item.autoReuse = true;
		}
		public override bool CanUseItem (Player player) {
			Explode.New(Main.MouseWorld,Main.rand.NextFloat(0.1f,0.7f));
			return base.CanUseItem(player);
		}
	} 
}