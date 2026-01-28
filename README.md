<img width="1890" height="709" alt="Logo_transparent" src="https://github.com/user-attachments/assets/e54dff8a-a320-4be9-a983-09b3c21b7eae" />

![Unity](https://img.shields.io/badge/Unity-100000?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)

> **"Play as a heavily armed mouse fighting back against feline oppression."**

**Gouda Luck!** is a comedic top-down twin-stick shooter developed for PC and Steam Deck. Blast through waves of hungry cats using your trusty Cheese Gun, collect 'Cheddar Points', and upgrade your arsenal to turn the hunter into the hunted.

---

## Game Overview
* **Genre:** Top-Down Twin-Stick Shooter / Roguelite
* **Platform:** PC / Steam Deck (1280x800)
* **Engine:** Unity 6
* **Status:** In Development (Student Project)

### Core Features
* **Twin-Stick Action:** Precise movement and aiming designed specifically for Steam Deck controls.
* **The Cheese Armory:** Unlock weapons like the **Cheddar-19**, **Mozza-MP5**, and **Magnum Gouda**.
* **Smart Enemy AI:** Cats utilise a Finite State Machine (FSM) to Chase and Attack.
* **Progression System:** Collect 'Cheddar Points' to upgrade fire rate and damage between runs.

---

## Controls (Steam Deck / Gamepad)

| Input | Action |
| :--- | :--- |
| **Left Stick** | Move Character |
| **Right Stick** | Aim / Rotate Character |
| **Right Trigger (R2)** | Fire Cheese Gun |
| **Left Trigger (L2)** | **ULTIMATE:** The Big Cheese (Area Blast) |
| **Button A** | Dash / Interact |

---

## Technical Details
This project focuses on **Refactoring** and **Clean Architecture**.
* **Refactored Mechanics:** The shooting system (Projectile instantiation & pooling) is refactored from a previous "Target the Weakest" prototype.
* **Finite State Machine:** Enemy behavior is driven by a modular FSM script.

---

## How to Play (Dev Build) (coming soon)
1.  Clone this repository to your local machine.
    ```bash
    git clone [https://github.com/lilnoggi/GoudaLuck_Unity_Project.git](https://github.com/lilnoggi/GoudaLuck_Unity_Project.git)
    ```
2.  Open **Unity Hub** and add the cloned folder.
3.  Open the project (Ensure you are using Unity Version 6 or compatible).
4.  Open the scene: `Scenes/KitchenLevel_01`.
5.  Press **Play**!

---

## Credits
* **Developer:** Amani Howe
* *University Project Module 4003*
