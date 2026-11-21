# Tsakel

Juego de fotografía de fauna de Chiapas en primera persona, hecho por **XoloMotion**. El jugador recorre la selva, levanta su cámara y fotografía a los animales para completar su álbum.

## Requisitos

- **Unity 6000.3.23f1** (Unity 6.3) con URP.
- Se juega con mouse y teclado. El código también lee un joystick para mirar.

## Cómo correrlo

1. Abre la carpeta del proyecto desde Unity Hub.
2. Abre `Assets/Scenes/MainMenu.unity` y dale Play.
3. Para un build, las escenas ya están en orden en *Build Profiles*: `MainMenu` y luego `Nivel1`.

## Cómo se juega

- Cada ronda elige 3 de las 5 especies. Cada una necesita 3 fotos válidas.
- La primera foto válida de una especie desbloquea su ficha.
- Una foto es válida si el animal está a menos de 25 m, ocupa al menos el 20 % del visor y no está tapado.
- Al completar las 3 especies sale el resumen de la ronda. El álbum se conserva mientras el juego esté abierto (no se guarda en disco).

| Acción | Control |
|---|---|
| Caminar | W A S D |
| Mirar | Mouse |
| Levantar la cámara | Clic derecho (mantener) o C |
| Zoom | Rueda del mouse o Q / E |
| Tomar la foto | Clic izquierdo o Espacio |
| Álbum | Tab |
| Pausa | Esc |

## Estructura

```
Assets/
  Scenes/            MainMenu y Nivel1
  Scripts/
    Album/           AlbumData (fotos en memoria), AlbumView, SpeciesCatalog (textos de cada especie)
    Animals/         AnimalSubject (animal fotografiable; se esconde y reaparece)
    Audio/           AudioVolume (aplica los volúmenes de Ajustes a un AudioSource)
    Environment/     BillboardVegetation (los sprites de vegetación miran a la cámara)
    Game/            GameFlow (ficha, álbum, pausa, resultados), RoundManager, RoundState, GameSettings
    Photography/     PhotoCameraController (visor, zoom, captura), PhotoEvaluator (reglas de foto válida)
    Player/          PlayerController (primera persona con CharacterController)
    UI/              GameUI (HUD), MainMenu, UIKit (la interfaz se construye por código)
  Resources/UI/      Logo.png (lo carga el menú principal)
  Modelos 3D/        Animales y objetos
  Sprites/           Fondos y vegetación 2D
  Input/             TsakelControls (acciones del jugador)
```

### Agregar un animal

1. Agrega una entrada en `SpeciesCatalog.cs` con un `id` nuevo.
2. Pon el modelo en `Nivel1` y agrégale el componente `AnimalSubject` con ese mismo `id`.
3. Opcional: asígnale varios `spots` para que reaparezca en lugares distintos.

### Agregar sonido

1. Crea un `AudioSource` con el clip (ambiente, música o sonido de un animal).
2. Agrégale el componente `AudioVolume` y elige el canal: `Animales` o `Musica`.
3. El componente aplica el volumen del menú de Ajustes, y se actualiza si el jugador lo cambia. El volumen maestro ya se aplica a todo el juego.

## Equipo

| Integrante | Rol |
|---|---|
| María Fernanda Estrada Gutiérrez | Director Creativo |
| Yajcdiel Colorado Vargas | Diseñador Gráfico (Supervisor) |
| Joaquín Miguel Palafox Ruíz | Programador (Supervisor) |
| Dulce Mariel Martinez Uriostegui | Diseñador Audiovisual |
| Jaime Antonio Navarro Trujeque | Programador |
| Irene Juarez Coutiño | Diseñador Gráfico |

## Modelos 3D de terceros

Las licencias CC BY obligan a dar crédito donde se comparta el juego, también en la pantalla de créditos. Completen lo que falta:

| Modelo | Carpeta | Autor y fuente | Licencia |
|---|---|---|---|
| Jaguar | `Modelos 3D/Animales/jaguar` | [LilEbo, Sketchfab](https://sketchfab.com/3d-models/jaguar-697bfa2edc674bc29817392f918d9b8f) | CC BY 4.0 (ver `license.txt`) |
| Tapir | `Modelos 3D/Animales/tapir-non-commercial` | Pendiente | Pendiente (la carpeta dice "non-commercial") |
| Guacamaya | `Modelos 3D/Animales/Guacamaya` | Pendiente | Pendiente |
| Ave (usada como quetzal) | `Modelos 3D/Animales/bird-orange` | Pendiente | Pendiente |
| Lagartija | `Modelos 3D/Animales/dragon-de-torres-mura` | Pendiente | Pendiente |
| Cerca de cuerda | `Modelos 3D/Objetos/rope-fence` | Pendiente | Pendiente |

Guarden el `license.txt` de cada modelo en su carpeta, como el del jaguar.

## Cómo trabajar con Git

- Hagan commits pequeños, con su propia cuenta y con un mensaje que diga qué cambió. Así se ve quién hizo qué.
- Antes de empezar, hagan `git pull`. Si dos personas editan la misma escena a la vez, va a haber conflictos difíciles de resolver: pónganse de acuerdo quién toca `Nivel1`.
- Suban siempre el `.meta` junto con su archivo. Si falta, Unity crea otro y se rompen las referencias.
- `Library/`, `Temp/`, `Logs/`, `UserSettings/` y `.DS_Store` ya están en el `.gitignore`.
- Los modelos y texturas pesan mucho. Si el repo crece más, conviene usar [Git LFS](https://git-lfs.com) para `.fbx`, `.png` y `.jpg`. Tienen que instalarlo todos antes de activarlo.
