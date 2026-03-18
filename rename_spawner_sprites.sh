#!/bin/bash
# ============================================================
# KacAtaKac — Spawner Sprite Yeniden Adlandirma Scripti
# ------------------------------------------------------------
# Sprites/Spawner/ altindaki frame_X_delay-0.2s.gif dosyalarini
# Yon_Attack_X.gif formatina cevirir.
# Ornek: East/frame_0_delay-0.2s.gif → East/East_Attack_1.gif
#
# KULLANIM: Unity KAPALI iken calistir!
# cd /Users/atakaan/KacAtaKac
# bash rename_spawner_sprites.sh
# ============================================================

SPAWNER_DIR="/Users/atakaan/KacAtaKac/Assets/Sprites/Spawner"

echo "Spawner sprite yeniden adlandirma basliyor..."
echo "Hedef klasor: $SPAWNER_DIR"
echo ""

# Her yon klasoru icin
for dir in "$SPAWNER_DIR"/*/; do
    dir_name=$(basename "$dir")

    # Sadece yon klasorlerini isle (meta dosyalari degil)
    if [[ "$dir_name" == *.meta ]]; then
        continue
    fi

    echo "--- $dir_name isleniyor ---"
    frame_index=1

    # frame_X_delay-0.2s.gif dosyalarini sirayla isle
    for old_file in "$dir"frame_*_delay-*.gif; do
        # Dosya yoksa atla (glob eslesmediyse)
        [ -f "$old_file" ] || continue

        new_name="${dir_name}_Attack_${frame_index}.gif"
        new_file="${dir}${new_name}"

        echo "  $old_file  →  $new_name"
        mv "$old_file" "$new_file"

        # Eski .meta dosyasini sil (Unity yeni isimle yeniden olusturacak)
        old_meta="${old_file}.meta"
        if [ -f "$old_meta" ]; then
            rm "$old_meta"
            echo "  (eski .meta silindi)"
        fi

        frame_index=$((frame_index + 1))
    done

    echo ""
done

echo "============================================"
echo "Tamamlandi! Simdi Unity'yi ac ve reimport et."
echo "Assets'e sag tik > Reimport All"
echo "============================================"
