# Liquid Dispenser Automation System

A sophisticated **liquid handling simulator** that automates the aspiration and dispensing workflow for high-throughput laboratory applications. The system precisely controls an eight-tip pipette head to transfer reagents from a standard source plate into custom microfluidic chips.

![Liquid_Dispenser_Demo](https://github.com/user-attachments/assets/157c0a8d-9e6f-4808-b11b-87e42008694c)


## Key Features

- **Flexible Source Plate Support**: Compatible with **96-well** and **384-well** standard microplate formats
- **Multiple Chip Configurations**: Supports **48×48**, **64×64**, and **96×96** microfluidic chips with extensible architecture for custom formats
- **Intelligent Tip Alignment**: Only tips aligned with destination wells participate in the dispensing operation, preventing cross-contamination and reagent waste
- **Multi-Well Dispensing**: Each column dispenses into **30 wells per tip**, enabling efficient batch processing across the chip
- **Real-time Visualization**: Screen captures show the complete aspiration and dispensing cycle with clear feedback on tip positioning and well targeting
- **Configurable Platform**: Easily extend to support any plate or chip geometry through modular design

## Technical Highlights

- **Eight-Tip Head Control**: Manages simultaneous multi-channel operations
- **Selective Dispensing Logic**: Validates tip-to-well alignment before liquid transfer
- **Asynchronous Operations**: Built with async/await patterns for responsive UI during long-running tasks
- **Cancellation Support**: Implements proper `CancellationToken` handling for graceful operation interruption
- **Extensible Architecture**: Generic plate and chip models allow configuration for any format

## Skills Demonstrated

- **C# 10, .NET 10, WPF, MVVM, Async/Await**
- **Dependency Injection** (Microsoft.Extensions.Hosting)
- **Domain-Driven Design**, Interface Polymorphism
- **MSTest unit testing** (simulator math)

## Use Case

Ideal for **high-throughput screening**, **compound library preparation**, and **parallel sample processing** in pharmaceutical research, genomics, and biotech applications.
