#!/usr/bin/env python3
"""
Instagram Reel/Post Generator for Mantu Games
Creates Instagram Reels/Posts from game screenshots
Supports random selection, custom captions, hashtags, and Instagram posting
"""

import os
import random
import json
import argparse
from pathlib import Path
from datetime import datetime
from typing import List, Dict, Optional

# Handle both moviepy v1 and v2 imports
try:
    # moviepy v1.x
    from moviepy.editor import ImageClip, AudioFileClip, concatenate_videoclips, TextClip, CompositeVideoClip, VideoFileClip
    from moviepy.video.fx.resize import resize
except ImportError:
    try:
        # moviepy v2.x
        from moviepy import ImageClip, AudioFileClip, concatenate_videoclips, TextClip, CompositeVideoClip, VideoFileClip
        from moviepy.video.fx import Resize
        resize = Resize
    except ImportError:
        print("Installing moviepy...")
        import subprocess
        subprocess.run(["pip", "install", "moviepy"], check=True)
        try:
            from moviepy.editor import ImageClip, AudioFileClip, concatenate_videoclips, TextClip, CompositeVideoClip, VideoFileClip
            from moviepy.video.fx.resize import resize
        except ImportError:
            from moviepy import ImageClip, AudioFileClip, concatenate_videoclips, TextClip, CompositeVideoClip, VideoFileClip
            from moviepy.video.fx import Resize
            resize = Resize

try:
    import requests
except ImportError:
    print("Installing requests...")
    import subprocess
    subprocess.run(["pip", "install", "requests"], check=True)
    import requests

# ─── Configuration ───
SCREENSHOTS_DIR = Path("~/Downloads/MantuGames-PlayAssets/screenshots").expanduser()
OUTPUT_DIR = Path("~/Downloads/MantuGames-PlayAssets/instagram_output").expanduser()
ASSETS_DIR = Path("~/Downloads/MantuGames-PlayAssets/store_assets").expanduser()

# Game metadata for captions/hashtags
GAME_META = {
    "sudoku": {"name": "Sudoku", "emoji": "🧩", "color": "#22d3ee", "hashtags": ["sudoku", "puzzle", "brain", "logic"]},
    "memory": {"name": "Card Memory", "emoji": "🃏", "color": "#34d399", "hashtags": ["memory", "cards", "brain", "match"]},
    "maze": {"name": "Maze Runner", "emoji": "🌀", "color": "#3b82f6", "hashtags": ["maze", "puzzle", "run", "escape"]},
    "wordfinder": {"name": "Word Finder", "emoji": "🔤", "color": "#a855f7", "hashtags": ["wordfinder", "words", "vocabulary", "search"]},
    "math": {"name": "Math Challenge", "emoji": "🧮", "color": "#f59e0b", "hashtags": ["math", "challenge", "arithmetic", "brain"]},
    "hanoi": {"name": "Tower of Hanoi", "emoji": "🗼", "color": "#ef4444", "hashtags": ["hanoi", "tower", "puzzle", "strategy"]},
    "puzzlepets": {"name": "Puzzle Pets", "emoji": "🐾", "color": "#f472b6", "hashtags": ["puzzle", "pets", "cute", "slide"]},
    "block": {"name": "Block Puzzle", "emoji": "🧱", "color": "#f97316", "hashtags": ["block", "puzzle", "tetris", "stack"]},
    "animalcrush": {"name": "Animal Crush", "emoji": "🦁", "color": "#ef4444", "hashtags": ["match3", "animals", "crush", "combo"]},
}

# Instagram config (fill these in)
INSTAGRAM_CONFIG_FILE = Path("~/.config/mantugames/instagram_config.json").expanduser()

DEFAULT_HASHTAGS = [
    "#MantuGames", "#BrainGames", "#PuzzleGames", "#MobileGaming",
    "#IndieGame", "#FreeGames", "#OfflineGames", "#BrainTraining",
    "#PuzzleLovers", "#MobileGames", "#GameDev", "#IndieDev"
]

CAPTION_TEMPLATES = [
    "Just crushed this level in {game_name}! {emoji} Who's faster? 🏃‍♂️💨",
    "Brain workout complete! {game_name} level cleared {emoji} 💪",
    "Can you beat my score? {game_name} challenge accepted! {emoji} 🎮",
    "Daily brain workout: {game_name} ✓ {emoji} 🧠✨",
    "Stuck on this level for ages... finally cracked it! {game_name} {emoji} 😤",
    "One more level... okay maybe five more {game_name} {emoji} 😅",
    "Free, offline, zero ads — just pure brain fun {game_name} {emoji} 🎁",
]

# ─── Utility Functions ───
def load_instagram_config() -> Dict:
    """Load Instagram API credentials from config file"""
    if INSTAGRAM_CONFIG_FILE.exists():
        with open(INSTAGRAM_CONFIG_FILE) as f:
            return json.load(f)
    return {}

def save_instagram_config(config: Dict):
    """Save Instagram API credentials"""
    INSTAGRAM_CONFIG_FILE.parent.mkdir(parents=True, exist_ok=True)
    with open(INSTAGRAM_CONFIG_FILE, 'w') as f:
        json.dump(config, f, indent=2)

def get_screenshots() -> List[Path]:
    """Get all screenshot files from screenshots directory"""
    if not SCREENSHOTS_DIR.exists():
        print(f"⚠️  Screenshots directory not found: {SCREENSHOTS_DIR}")
        return []
    extensions = {'.png', '.jpg', '.jpeg', '.webp'}
    return [f for f in SCREENSHOTS_DIR.iterdir() if f.suffix.lower() in extensions]

def detect_game_from_filename(filename: str) -> Optional[str]:
    """Detect game name from screenshot filename"""
    name = filename.lower()
    for game_key in GAME_META.keys():
        if game_key in name:
            return game_key
    return None

def generate_caption(game_key: Optional[str], is_reel: bool = True) -> str:
    """Generate a caption with hashtags"""
    if game_key and game_key in GAME_META:
        meta = GAME_META[game_key]
        game_name = meta["name"]
        emoji = meta["emoji"]
        game_tags = " ".join(f"#{tag}" for tag in meta["hashtags"])
    else:
        game_name = "Mantu Games"
        emoji = "🎮"
        game_tags = ""

    template = random.choice(CAPTION_TEMPLATES)
    caption = template.format(game_name=game_name, emoji=emoji)

    tags = " ".join(DEFAULT_HASHTAGS)
    if game_tags:
        tags = f"{game_tags} {tags}"

    if is_reel:
        caption += f"\n\n🎬 Watch the full playthrough!\n\n{tags}"
    else:
        caption += f"\n\n{tags}"

    return caption

# ─── Video Creation ───
def create_reel_from_screenshots(
    screenshots: List[Path],
    game_key: Optional[str],
    output_path: Path,
    duration_per_image: float = 1.5,
    fps: int = 30,
    music_path: Optional[Path] = None
) -> Path:
    """Create a Reel video from screenshots"""
    if not screenshots:
        raise ValueError("No screenshots provided")

    clips = []
    for img_path in screenshots:
        clip = ImageClip(str(img_path)).set_duration(duration_per_image)
        clips.append(clip)

    video = concatenate_videoclips(clips, method="compose")

    # Add game title overlay
    if game_key and game_key in GAME_META:
        meta = GAME_META[game_key]
        title = f"{meta['emoji']} {meta['name']}"
        title_clip = TextClip(
            title, fontsize=48, color='white',
            font='Orbitron-Bold', stroke_color='black', stroke_width=2
        ).set_position(('center', 50)).set_duration(video.duration)
        video = CompositeVideoClip([video, title_clip])

    # Add Mantu Games watermark
    watermark = TextClip(
        "Mantu Games", fontsize=24, color='white',
        font='Orbitron', stroke_color='black', stroke_width=1
    ).set_position(('right', 'bottom')).set_duration(video.duration).margin(right=20, bottom=20, opacity=0.7)
    video = CompositeVideoClip([video, watermark])

    # Add music if provided
    if music_path and music_path.exists():
        audio = AudioFileClip(str(music_path)).subclip(0, video.duration)
        video = video.set_audio(audio)

    # Ensure 9:16 aspect ratio for Reels
    video = video.resize((1080, 1920))

    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    video.write_videofile(str(output_path), fps=fps, codec='libx264', audio_codec='aac')
    return output_path

def create_carousel_post(screenshots: List[Path], game_key: Optional[str], output_dir: Path) -> List[Path]:
    """Create individual images for carousel post"""
    output_dir.mkdir(parents=True, exist_ok=True)
    output_paths = []

    for i, img_path in enumerate(screenshots):
        # Could add overlay text here if needed
        output_path = output_dir / f"carousel_{i+1:02d}.jpg"
        # For now just copy, could add branding with PIL
        import shutil
        shutil.copy2(img_path, output_path)
        output_paths.append(output_path)

    return output_paths

# ─── Instagram Posting ───
class InstagramPublisher:
    def __init__(self, config: Dict):
        self.config = config
        self.access_token = config.get('access_token')
        self.user_id = config.get('user_id')
        self.base_url = "https://graph.facebook.com/v18.0"

    def is_configured(self) -> bool:
        return bool(self.access_token and self.user_id)

    def upload_media(self, media_path: Path, caption: str, media_type: str = 'REELS') -> Dict:
        """Upload media to Instagram via Graph API"""
        if not self.is_configured():
            raise RuntimeError("Instagram not configured. Run setup first.")

        url = f"{self.base_url}/{self.user_id}/media"
        data = {
            'media_type': media_type,
            'caption': caption,
            'access_token': self.access_token,
        }

        if media_type == 'REELS':
            # For Reels, upload video first
            with open(media_path, 'rb') as f:
                files = {'video': f}
                data['media_type'] = 'REELS'
                response = requests.post(url, data=data, files=files)
        else:
            # For carousel/images
            data['image_url'] = f"file://{media_path}"  # Need to upload to hosting first
            response = requests.post(url, data=data)

        return response.json()

    def publish_media(self, creation_id: str) -> Dict:
        """Publish uploaded media"""
        url = f"{self.base_url}/{self.user_id}/media_publish"
        data = {
            'creation_id': creation_id,
            'access_token': self.access_token,
        }
        return requests.post(url, data=data).json()

# ─── Main CLI ───
def main():
    parser = argparse.ArgumentParser(description="Generate Instagram Reels/Posts from game screenshots")
    parser.add_argument('--reel', action='store_true', help='Create a Reel video')
    parser.add_argument('--post', action='store_true', help='Create carousel post images')
    parser.add_argument('--game', help='Specific game key (sudoku, memory, maze, etc.)')
    parser.add_argument('--random', action='store_true', help='Randomly pick a game')
    parser.add_argument('--all', action='store_true', help='Create for all games')
    parser.add_argument('--post-to-insta', action='store_true', help='Upload to Instagram')
    parser.add_argument('--duration', type=float, default=1.5, help='Seconds per image in Reel')
    parser.add_argument('--music', help='Background music file path')
    parser.add_argument('--output', help='Custom output filename')
    parser.add_argument('--setup-insta', action='store_true', help='Configure Instagram API credentials')

    args = parser.parse_args()

    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    if args.setup_insta:
        setup_instagram()
        return

    screenshots = get_screenshots()
    if not screenshots:
        print("❌ No screenshots found in", SCREENSHOTS_DIR)
        return

    # Filter by game if specified
    if args.game:
        screenshots = [s for s in screenshots if args.game in s.name.lower()]
        game_key = args.game
    elif args.random:
        screenshot = random.choice(screenshots)
        screenshots = [screenshot]
        game_key = detect_game_from_filename(screenshot.name)
    elif args.all:
        game_key = None
    else:
        # Default: pick random
        screenshot = random.choice(screenshots)
        screenshots = [screenshot]
        game_key = detect_game_from_filename(screenshot.name)

    if not screenshots:
        print(f"❌ No screenshots found for game: {args.game}")
        return

    print(f"🎮 Processing {len(screenshots)} screenshot(s) for game: {game_key or 'mixed'}")

    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    game_suffix = f"_{game_key}" if game_key else "_mixed"

    if args.reel:
        output_name = args.output or f"reel_{game_suffix}_{timestamp}.mp4"
        output_path = OUTPUT_DIR / output_name
        music_path = Path(args.music).expanduser() if args.music else None

        print(f"🎬 Creating Reel: {output_path}")
        video_path = create_reel_from_screenshots(
            screenshots, game_key, output_path,
            duration_per_image=args.duration,
            music_path=music_path
        )
        print(f"✅ Reel created: {video_path}")

        caption = generate_caption(game_key, is_reel=True)
        print(f"\n📝 Caption:\n{caption}")

        if args.post_to_insta:
            post_to_instagram(video_path, caption, 'REELS')

    if args.post:
        print(f"📸 Creating carousel post images...")
        output_dir = OUTPUT_DIR / f"carousel{game_suffix}_{timestamp}"
        paths = create_carousel_post(screenshots, game_key, output_dir)
        print(f"✅ Created {len(paths)} carousel images in {output_dir}")

        caption = generate_caption(game_key, is_reel=False)
        print(f"\n📝 Caption:\n{caption}")

        if args.post_to_insta:
            print("⚠️  Carousel posting requires manual upload or API with media container")

    print("\n✨ Done!")

def setup_instagram():
    """Interactive Instagram API setup"""
    print("🔧 Instagram Graph API Setup")
    print("=" * 40)
    print("You need:")
    print("1. Facebook Developer App: https://developers.facebook.com/")
    print("2. Instagram Business/Creator account connected to Facebook Page")
    print("3. App with 'instagram_graph_user_profile', 'instagram_content_publish' permissions")
    print()

    user_id = input("Instagram Business Account ID (numeric): ").strip()
    access_token = input("Long-lived Access Token: ").strip()

    config = {
        'user_id': user_id,
        'access_token': access_token
    }

    save_instagram_config(config)
    print("✅ Saved to", INSTAGRAM_CONFIG_FILE)

def post_to_instagram(media_path: Path, caption: str, media_type: str = 'REELS'):
    """Helper to post to Instagram"""
    config = load_instagram_config()
    publisher = InstagramPublisher(config)
    if not publisher.is_configured():
        print("❌ Instagram not configured. Run: python generate_instagram_reels.py --setup-insta")
        return

    print(f"📤 Uploading {media_type}...")
    try:
        result = publisher.upload_media(Path(media_path), caption, media_type)
        if 'id' in result:
            print(f"✅ Uploaded! Creation ID: {result['id']}")
            pub_result = publisher.publish_media(result['id'])
            print(f"✅ Published: {pub_result}")
        else:
            print(f"❌ Upload failed: {result}")
    except Exception as e:
        print(f"❌ Error: {e}")

if __name__ == "__main__":
    main()