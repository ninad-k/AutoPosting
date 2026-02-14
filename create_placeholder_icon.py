from PIL import Image, ImageDraw, ImageFont
import os

def create_placeholder_icon(output_path):
    try:
        # Create a blue image
        img = Image.new('RGB', (256, 256), color = (0, 120, 215))
        d = ImageDraw.Draw(img)
        
        # Draw a white "SM" text or simple shape
        # Since we might not have fonts, let's draw a simple monitor shape
        
        # Monitor stand
        d.rectangle([100, 200, 156, 220], fill=(200, 200, 200))
        d.rectangle([80, 220, 176, 230], fill=(200, 200, 200))
        
        # Monitor screen border
        d.rectangle([30, 50, 226, 200], fill=(50, 50, 50))
        
        # Monitor screen content
        d.rectangle([40, 60, 216, 190], fill=(255, 255, 255))
        
        # "SM" Text roughly
        d.text((100, 100), "SM", fill=(0, 120, 215))

        img.save(output_path)
        print(f"Created placeholder icon at {output_path}")
        
    except Exception as e:
        print(f"Error creating placeholder: {e}")

if __name__ == "__main__":
    create_placeholder_icon("icon.png")
