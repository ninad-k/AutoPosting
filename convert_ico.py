from PIL import Image
import sys
import os

def convert_to_ico(input_path, output_path):
    try:
        if not os.path.exists(input_path):
            print(f"Error: Input file '{input_path}' not found.")
            return
            
        img = Image.open(input_path)
        img.save(output_path, format='ICO', sizes=[(256, 256)])
        print(f"Successfully converted '{input_path}' to '{output_path}'")
    except Exception as e:
        print(f"Error converting image: {e}")

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python convert_ico.py <input_png> <output_ico>")
    else:
        convert_to_ico(sys.argv[1], sys.argv[2])
